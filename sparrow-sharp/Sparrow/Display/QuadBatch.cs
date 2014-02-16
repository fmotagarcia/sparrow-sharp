using System;
using Sparrow.Utils;

namespace Sparrow.Display
{
	// TODO check if dealloc exists
	public class QuadBatch
	{
		private int _numQuads;
		private bool _syncRequired;

//		Texture *_texture;
		private bool _premultipliedAlpha;
		private bool _tinted;

//		BaseEffect *_baseEffect;
		private VertexData _vertexData;
		private int _vertexBufferName;
		private List<double> _indexData;
		private int _indexBufferName;

		public QuadBatch ()
		{
			_numQuads = 0;
			_syncRequired = false;
			_vertexData = new VertexData ();
//			_baseEffect = [[SPBaseEffect alloc] init];
		}

		public void Reset() {
			_numQuads = 0;
			_syncRequired = true;
			//TODO set texture to null
		}


		public void AddQuad(Quad quad) {
			AddQuad (quad, quad.Alpha, BlendMode.AUTO, null);
		}

		public void AddQuad(Quad quad, double alpha) {
			// TOOD add blendMode to quad
			AddQuad (quad, alpha, Sparrow.Display.BlendMode.AUTO, null);
		}

		public void AddQuad(Quad quad, double alpha, uint blendMode) {
			AddQuad (quad, quad.Alpha, blendMode, null);
		}

		public void AddQuad(Quad quad, double alpha, uint blendMode, Matrix matrix) {
			if (matrix == null) {
				matrix = quad.TransformationMatrix;
			}

			if (_numQuads == 0)
			{
				_premultipliedAlpha = quad.PremultipliedAlpha;
				BlendMode = blendMode;
				_vertexData.PremultipliedAlpha = _premultipliedAlpha;
			}

			int vertexID = _numQuads * 4;

			quad.CopyVertexDataTo (_vertexData, vertexID);
			_vertexData.TransformVerticesWithMatrix (matrix, vertexID, 4);

			if (alpha != 1.0) {
				_vertexData.ScaleAlphaBy (alpha, vertexID, 4);
			}

			if (!_tinted) {
				_tinted = alpha != 1.0d || quad.Tinted;
			}

			_syncRequired = true;
			_numQuads++;
		}

		public bool IsStateChange(bool tinted, Texture texture, double alpha, bool premultipliedAlpha, uint blendMode, int numQuads) {
			if (_numQuads == 0) {
				return false;
			}
		}
	}
}



- (BOOL)isStateChangeWithTinted:(BOOL)tinted texture:(SPTexture *)texture alpha:(float)alpha
premultipliedAlpha:(BOOL)pma blendMode:(uint)blendMode numQuads:(int)numQuads
{
	if (_numQuads == 0) return NO;
	else if (_numQuads + numQuads > 8192) return YES; // maximum buffer size
	else if (!_texture && !texture)
		return _premultipliedAlpha != pma || self.blendMode != blendMode;
	else if (_texture && texture)
		return _tinted != (tinted || alpha != 1.0f) ||
			_texture.name != texture.name ||
			self.blendMode != blendMode;
	else return YES;
}

- (SPRectangle *)boundsInSpace:(SPDisplayObject *)targetSpace
{
	SPMatrix *matrix = targetSpace == self ? nil : [self transformationMatrixToSpace:targetSpace];
	return [_vertexData boundsAfterTransformation:matrix atIndex:0 numVertices:_numQuads*4];
}

- (void)render:(SPRenderSupport *)support
{
	if (_numQuads)
	{
		[support finishQuadBatch];
		[support addDrawCalls:1];
		[self renderWithMvpMatrix:support.mvpMatrix alpha:support.alpha blendMode:support.blendMode];
	}
}

- (void)renderWithMvpMatrix:(SPMatrix *)matrix
{
	[self renderWithMvpMatrix:matrix alpha:1.0f blendMode:self.blendMode];
}

- (void)renderWithMvpMatrix:(SPMatrix *)matrix alpha:(float)alpha blendMode:(uint)blendMode;
{
	if (!_numQuads) return;
	if (_syncRequired) [self syncBuffers];
	if (blendMode == SPBlendModeAuto)
		[NSException raise:SPExceptionInvalidOperation
			format:@"			cannot render object with blend mode AUTO"];

	_baseEffect.texture = _texture;
	_baseEffect.premultipliedAlpha = _premultipliedAlpha;
	_baseEffect.mvpMatrix = matrix;
	_baseEffect.useTinting = _tinted || alpha != 1.0f;
	_baseEffect.alpha = alpha;

	[_baseEffect prepareToDraw];

	[SPBlendMode applyBlendFactorsForBlendMode:blendMode premultipliedAlpha:_premultipliedAlpha];

	int attribPosition  = _baseEffect.attribPosition;
	int attribColor     = _baseEffect.attribColor;
	int attribTexCoords = _baseEffect.attribTexCoords;

	glEnableVertexAttribArray(attribPosition);
	glEnableVertexAttribArray(attribColor);

	if (_texture)
		glEnableVertexAttribArray(attribTexCoords);

	glBindBuffer(GL_ARRAY_BUFFER, _vertexBufferName);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferName);

	glVertexAttribPointer(attribPosition, 2, GL_FLOAT, GL_FALSE, sizeof(SPVertex),
		(void *)(offsetof(SPVertex, position)));

	glVertexAttribPointer(attribColor, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(SPVertex),
		(void *)(offsetof(SPVertex, color)));

	if (_texture)
	{
		glVertexAttribPointer(attribTexCoords, 2, GL_FLOAT, GL_FALSE, sizeof(SPVertex),
			(void *)(offsetof(SPVertex, texCoords)));
	}

	int numIndices = _numQuads * 6;
	glDrawElements(GL_TRIANGLES, numIndices, GL_UNSIGNED_SHORT, 0);
}

#pragma mark Compilation Methods

+ (NSMutableArray *)compileObject:(SPDisplayObject *)object
{
	return [self compileObject:object intoArray:nil];
}

+ (NSMutableArray *)compileObject:(SPDisplayObject *)object intoArray:(NSMutableArray *)quadBatches
{
	if (!quadBatches) quadBatches = [NSMutableArray array];

	[self compileObject:object intoArray:quadBatches atPosition:-1
		withMatrix:[SPMatrix matrixWithIdentity] alpha:1.0f blendMode:SPBlendModeAuto];

	return quadBatches;
}

+ (int)compileObject:(SPDisplayObject *)object intoArray:(NSMutableArray *)quadBatches
atPosition:(int)quadBatchID withMatrix:(SPMatrix *)transformationMatrix
alpha:(float)alpha blendMode:(uint)blendMode
{
	BOOL isRootObject = NO;
	float objectAlpha = object.alpha;

	SPQuad *quad = [object isKindOfClass:[SPQuad class]] ? (SPQuad *)object : nil;
	SPQuadBatch *batch = [object isKindOfClass:[SPQuadBatch class]] ? (SPQuadBatch *)object :nil;
	SPDisplayObjectContainer *container = [object isKindOfClass:[SPDisplayObjectContainer class]] ?
	                                      (SPDisplayObjectContainer *)object : nil;
	if (quadBatchID == -1)
	{
		isRootObject = YES;
		quadBatchID = 0;
		objectAlpha = 1.0f;
		blendMode = object.blendMode;
		if (quadBatches.count == 0) [quadBatches addObject:[SPQuadBatch quadBatch]];
		else [quadBatches[0] reset];
	}

	if (container)
	{
		SPDisplayObjectContainer *container = (SPDisplayObjectContainer *)object;
		SPMatrix *childMatrix = [SPMatrix matrixWithIdentity];

		for (SPDisplayObject *child in container)
		{
			if ([child hasVisibleArea])
			{
				uint childBlendMode = child.blendMode;
				if (childBlendMode == SPBlendModeAuto) childBlendMode = blendMode;

				[childMatrix copyFromMatrix:transformationMatrix];
				[childMatrix prependMatrix:child.transformationMatrix];
				quadBatchID = [self compileObject:child intoArray:quadBatches atPosition:quadBatchID
					withMatrix:childMatrix alpha:alpha * objectAlpha
					blendMode:childBlendMode];
			}
		}
	}
	else if (quad || batch)
	{
		SPTexture *texture = [(id)object texture];
		BOOL tinted = [(id)object tinted];
		BOOL pma = [(id)object premultipliedAlpha];
		int numQuads = batch ? batch.numQuads : 1;

		SPQuadBatch *currentBatch = quadBatches[quadBatchID];

		if ([currentBatch isStateChangeWithTinted:tinted texture:texture alpha:alpha * objectAlpha
			premultipliedAlpha:pma blendMode:blendMode numQuads:numQuads])
		{
			quadBatchID++;
			if (quadBatches.count <= quadBatchID) [quadBatches addObject:[SPQuadBatch quadBatch]];
			currentBatch = quadBatches[quadBatchID];
			[currentBatch reset];
		}

		if (quad)
			[currentBatch addQuad:quad alpha:alpha * objectAlpha blendMode:blendMode
				matrix:transformationMatrix];
		else
			[currentBatch addQuadBatch:batch alpha:alpha * objectAlpha blendMode:blendMode
				matrix:transformationMatrix];
	}
	else
	{
		[NSException raise:SPExceptionInvalidOperation format:@"		Unsupported display object: %@",
			[object class]];
	}

	if (isRootObject)
	{
		// remove unused batches
		for (int i=(int)quadBatches.count-1; i>quadBatchID; --i)
			[quadBatches removeLastObject];
	}

	return quadBatchID;
}

#pragma mark Private

- (void)expand
{
	int oldCapacity = self.capacity;
	self.capacity = oldCapacity < 8 ? 16 : oldCapacity * 2;
}

- (void)createBuffers
{
	[self destroyBuffers];

	int numVertices = _vertexData.numVertices;
	int numIndices = numVertices / 4 * 6;
	if (numVertices == 0) return;

	glGenBuffers(1, &_vertexBufferName);
	glGenBuffers(1, &_indexBufferName);

	if (!_vertexBufferName || !_indexBufferName)
		[NSException raise:SPExceptionOperationFailed format:@"		could not create vertex buffers"];

	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferName);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(ushort) * numIndices, _indexData, GL_STATIC_DRAW);

	_syncRequired = YES;
}

- (void)destroyBuffers
{
	if (_vertexBufferName)
	{
		glDeleteBuffers(1, &_vertexBufferName);
		_vertexBufferName = 0;
	}

	if (_indexBufferName)
	{
		glDeleteBuffers(1, &_indexBufferName);
		_indexBufferName = 0;
	}
}

- (void)syncBuffers
{
	if (!_vertexBufferName)
		[self createBuffers];

	// don't use 'glBufferSubData'! It's much slower than uploading
	// everything via 'glBufferData', at least on the iPad 1.

	glBindBuffer(GL_ARRAY_BUFFER, _vertexBufferName);
	glBufferData(GL_ARRAY_BUFFER, sizeof(SPVertex) * _vertexData.numVertices,
		_vertexData.vertices, GL_STATIC_DRAW);

	_syncRequired = NO;
}

- (int)capacity
{
	return _vertexData.numVertices / 4;
}

- (void)setCapacity:(int)newCapacity
{
	NSAssert(newCapacity > 0, @"	capacity must not be zero");

	int oldCapacity = self.capacity;
	int numVertices = newCapacity * 4;
	int numIndices  = newCapacity * 6;

	_vertexData.numVertices = numVertices;

	if (!_indexData) _indexData = malloc(sizeof(ushort) * numIndices);
	else             _indexData = realloc(_indexData, sizeof(ushort) * numIndices);

	for (int i=oldCapacity; i<newCapacity; ++i)
	{
		_indexData[i*6  ] = i*4;
		_indexData[i*6+1] = i*4 + 1;
		_indexData[i*6+2] = i*4 + 2;
		_indexData[i*6+3] = i*4 + 1;
		_indexData[i*6+4] = i*4 + 3;
		_indexData[i*6+5] = i*4 + 2;
	}

	[self destroyBuffers];
	_syncRequired = YES;
}

@end