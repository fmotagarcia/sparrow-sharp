using System;

namespace Sparrow.Text
{
    internal class MiniBitmapFont
    {
        // 128 x 64 png image, characters are max. is 5x5 pixels
        internal static readonly string MiniFontImageDataBase64 = 
            "iVBORw0KGgoAAAANSUhEUgAAAIAAAABABAMAAAAg+GJMAAAAJFBMVEUAAAD///////////////////////////////////" +
            "////////+0CY3pAAAAC3RSTlMAAgQGCg4QFNn5/aulndcAAANHSURBVFhH7ZYxrhtHEESf4J+9RLGu4NCRoHQBBZv5EEp8" +
            "AAVMfAQf4R+hAgIK6nIOenZJSt+GjW/IiRrN4XA4XV1dPcshvNrevFkubyFAELybfzshRATg3bvl4dkjNHw5YV6eKAkAz8" +
            "/LH23Q/41JIs3ptuO3FTydHAwakUYS3fabsyjfrZzROQHcdieQxDOrrc3yu8QLQG4ArbpI9HHjXzO4B0Cp2w75KtM3Gtz8" +
            "a4ARD0eV721zMhpyOoSix+wtJIKY20wgQAsjyw1SJMkxe9YpmtzPwCFAI4xaD0h/b3b2NkeD8NNv4qg5Q+y0926NOGfmad" +
            "qAK/d5YrZc9xk+5nqZgXNtywEwDCYOEfzlwyPAzjUzvAQw9a/gLA3GF/G7EsithHNtuvBakxFFqYlluh8xFut8yog69Mk6" +
            "MECmb7OS6xan03JUTSzw5XIjrfNakUc0SYjQ5gEg0Dl7lh45l+mHO4DrlgZCs9pfmuCW605z1W2V8DIDi2tpkRRiB0BeBD" +
            "gkCQmkpU1Yz4sUVm8zJVjiocGh2OrCgH5fa1szNDLVBwsWm3mjx9imjV01g7/+DFQGYCTjy+cFuRNy3ZKnhBk5PKNR22CS" +
            "SJL8npCVvdltJiuBPI3EpGnTALKORyKReThXaxaDI/c9g5wMcKGbeZ+WreKDJeReg8CdBq82UZykU6/tLC4/LznWb9fNEU" +
            "yNbruMjyzKdDWwNorO7PPFz5d1meEYHgxyA1j7oaU5qTBEZ8Ps7XGbZ+U/0wvBqRXBSQ+67eRBg5k3yMkDOe7YMN/euSPj" +
            "a+3IjRynwyNHhwqrGJyKmgYJdELDVGo7MOv/xK5bYQEUa8kpSyNhXTATnQyGVkurF9sBeMpVSQJzSWRffYWQA0No3Hb3ol" +
            "53wHuAOtUcDBh5uWkw39GgS4PSTglLI6EJyn9ggxMy/MZqJFJ7XIYNJwdJKzFgCfHiBcTDM6/tenFL8GOiW8oUUQjlWiCC" +
            "DEyOB+MGkAHYiW5hqTBi053pQKYYmXAX/dD1GNEJmxOc+xJGg+OILAlOgb6HqTHaEm2dmvLTHyRJiM7T2Kr9hp5BOmcrjH" +
            "wXwvv3ujr2dcijOSoMA1BCXLL+E5M5NT/sh/2v9idsZLc1sYX4WAAAAABJRU5ErkJggg==";

        internal static readonly string FontXML = @"<font><info face='mini' size='8' bold='0' italic='0' smooth='0' />
<common lineHeight='8' base='7' scaleW='128' scaleH='64' pages='1' packed='0' />
<chars count='191'>
<char id='195' x='1' y='1' width='5' height='9' xoffset='0' yoffset='-2' xadvance='6' />
<char id='209' x='7' y='1' width='5' height='9' xoffset='0' yoffset='-2' xadvance='6' />
<char id='213' x='13' y='1' width='5' height='9' xoffset='0' yoffset='-2' xadvance='6' />
<char id='253' x='19' y='1' width='4' height='9' xoffset='0' yoffset='0' xadvance='5' />
<char id='255' x='24' y='1' width='4' height='9' xoffset='0' yoffset='0' xadvance='5' />
<char id='192' x='29' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='193' x='35' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='194' x='41' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='197' x='47' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='200' x='53' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='201' x='59' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='202' x='65' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='210' x='71' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='211' x='77' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='212' x='83' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='217' x='89' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='218' x='95' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='219' x='101' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='221' x='107' y='1' width='5' height='8' xoffset='0' yoffset='-1' xadvance='6' />
<char id='206' x='113' y='1' width='3' height='8' xoffset='-1' yoffset='-1' xadvance='2' />
<char id='204' x='117' y='1' width='2' height='8' xoffset='-1' yoffset='-1' xadvance='2' />
<char id='205' x='120' y='1' width='2' height='8' xoffset='0' yoffset='-1' xadvance='2' />
<char id='36' x='1' y='11' width='5' height='7' xoffset='0' yoffset='1' xadvance='6' />
<char id='196' x='7' y='11' width='5' height='7' xoffset='0' yoffset='0' xadvance='6' />
<char id='199' x='13' y='11' width='5' height='7' xoffset='0' yoffset='2' xadvance='6' />
<char id='203' x='19' y='11' width='5' height='7' xoffset='0' yoffset='0' xadvance='6' />
<char id='214' x='25' y='11' width='5' height='7' xoffset='0' yoffset='0' xadvance='6' />
<char id='220' x='31' y='11' width='5' height='7' xoffset='0' yoffset='0' xadvance='6' />
<char id='224' x='37' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='225' x='42' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='226' x='47' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='227' x='52' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='232' x='57' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='233' x='62' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='234' x='67' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='235' x='72' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='241' x='77' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='242' x='82' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='243' x='87' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='244' x='92' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='245' x='97' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='249' x='102' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='250' x='107' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='251' x='112' y='11' width='4' height='7' xoffset='0' yoffset='0' xadvance='5' />
<char id='254' x='117' y='11' width='4' height='7' xoffset='0' yoffset='2' xadvance='5' />
<char id='123' x='122' y='11' width='3' height='7' xoffset='0' yoffset='1' xadvance='4' />
<char id='125' x='1' y='19' width='3' height='7' xoffset='0' yoffset='1' xadvance='4' />
<char id='167' x='5' y='19' width='3' height='7' xoffset='0' yoffset='1' xadvance='4' />
<char id='207' x='9' y='19' width='3' height='7' xoffset='-1' yoffset='0' xadvance='2' />
<char id='106' x='13' y='19' width='2' height='7' xoffset='0' yoffset='2' xadvance='3' />
<char id='40' x='16' y='19' width='2' height='7' xoffset='0' yoffset='1' xadvance='3' />
<char id='41' x='19' y='19' width='2' height='7' xoffset='0' yoffset='1' xadvance='3' />
<char id='91' x='22' y='19' width='2' height='7' xoffset='0' yoffset='1' xadvance='3' />
<char id='93' x='25' y='19' width='2' height='7' xoffset='0' yoffset='1' xadvance='3' />
<char id='124' x='28' y='19' width='1' height='7' xoffset='1' yoffset='1' xadvance='4' />
<char id='81' x='30' y='19' width='5' height='6' xoffset='0' yoffset='2' xadvance='6' />
<char id='163' x='36' y='19' width='5' height='6' xoffset='0' yoffset='1' xadvance='6' />
<char id='177' x='42' y='19' width='5' height='6' xoffset='0' yoffset='2' xadvance='6' />
<char id='181' x='48' y='19' width='5' height='6' xoffset='0' yoffset='3' xadvance='6' />
<char id='103' x='54' y='19' width='4' height='6' xoffset='0' yoffset='3' xadvance='5' />
<char id='112' x='59' y='19' width='4' height='6' xoffset='0' yoffset='3' xadvance='5' />
<char id='113' x='64' y='19' width='4' height='6' xoffset='0' yoffset='3' xadvance='5' />
<char id='121' x='69' y='19' width='4' height='6' xoffset='0' yoffset='3' xadvance='5' />
<char id='162' x='74' y='19' width='4' height='6' xoffset='0' yoffset='2' xadvance='5' />
<char id='228' x='79' y='19' width='4' height='6' xoffset='0' yoffset='1' xadvance='5' />
<char id='229' x='84' y='19' width='4' height='6' xoffset='0' yoffset='1' xadvance='5' />
<char id='231' x='89' y='19' width='4' height='6' xoffset='0' yoffset='3' xadvance='5' />
<char id='240' x='94' y='19' width='4' height='6' xoffset='0' yoffset='1' xadvance='5' />
<char id='246' x='99' y='19' width='4' height='6' xoffset='0' yoffset='1' xadvance='5' />
<char id='252' x='104' y='19' width='4' height='6' xoffset='0' yoffset='1' xadvance='5' />
<char id='238' x='109' y='19' width='3' height='6' xoffset='-1' yoffset='1' xadvance='2' />
<char id='59' x='113' y='19' width='2' height='6' xoffset='0' yoffset='3' xadvance='4' />
<char id='236' x='116' y='19' width='2' height='6' xoffset='-1' yoffset='1' xadvance='2' />
<char id='237' x='119' y='19' width='2' height='6' xoffset='0' yoffset='1' xadvance='2' />
<char id='198' x='1' y='27' width='9' height='5' xoffset='0' yoffset='2' xadvance='10' />
<char id='190' x='11' y='27' width='8' height='5' xoffset='0' yoffset='2' xadvance='9' />
<char id='87' x='20' y='27' width='7' height='5' xoffset='0' yoffset='2' xadvance='8' />
<char id='188' x='28' y='27' width='7' height='5' xoffset='0' yoffset='2' xadvance='8' />
<char id='189' x='36' y='27' width='7' height='5' xoffset='0' yoffset='2' xadvance='8' />
<char id='38' x='44' y='27' width='6' height='5' xoffset='0' yoffset='2' xadvance='7' />
<char id='164' x='51' y='27' width='6' height='5' xoffset='0' yoffset='2' xadvance='7' />
<char id='208' x='58' y='27' width='6' height='5' xoffset='0' yoffset='2' xadvance='7' />
<char id='8364' x='65' y='27' width='6' height='5' xoffset='0' yoffset='2' xadvance='7' />
<char id='65' x='72' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='66' x='78' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='67' x='84' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='68' x='90' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='69' x='96' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='70' x='102' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='71' x='108' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='72' x='114' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='75' x='120' y='27' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='77' x='1' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='78' x='7' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='79' x='13' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='80' x='19' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='82' x='25' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='83' x='31' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='84' x='37' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='85' x='43' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='86' x='49' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='88' x='55' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='89' x='61' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='90' x='67' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='50' x='73' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='51' x='79' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='52' x='85' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='53' x='91' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='54' x='97' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='56' x='103' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='57' x='109' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='48' x='115' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='47' x='121' y='33' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='64' x='1' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='92' x='7' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='37' x='13' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='43' x='19' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='35' x='25' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='42' x='31' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='165' x='37' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='169' x='43' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='174' x='49' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='182' x='55' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='216' x='61' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='247' x='67' y='39' width='5' height='5' xoffset='0' yoffset='2' xadvance='6' />
<char id='74' x='73' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='76' x='78' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='98' x='83' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='100' x='88' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='104' x='93' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='107' x='98' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='55' x='103' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='63' x='108' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='191' x='113' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='222' x='118' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='223' x='123' y='39' width='4' height='5' xoffset='0' yoffset='2' xadvance='5' />
<char id='116' x='1' y='45' width='3' height='5' xoffset='0' yoffset='2' xadvance='4' />
<char id='60' x='5' y='45' width='3' height='5' xoffset='0' yoffset='2' xadvance='4' />
<char id='62' x='9' y='45' width='3' height='5' xoffset='0' yoffset='2' xadvance='4' />
<char id='170' x='13' y='45' width='3' height='5' xoffset='0' yoffset='2' xadvance='4' />
<char id='186' x='17' y='45' width='3' height='5' xoffset='0' yoffset='2' xadvance='4' />
<char id='239' x='21' y='45' width='3' height='5' xoffset='-1' yoffset='2' xadvance='2' />
<char id='102' x='25' y='45' width='2' height='5' xoffset='0' yoffset='2' xadvance='3' />
<char id='49' x='28' y='45' width='2' height='5' xoffset='0' yoffset='2' xadvance='3' />
<char id='73' x='31' y='45' width='1' height='5' xoffset='0' yoffset='2' xadvance='2' />
<char id='105' x='33' y='45' width='1' height='5' xoffset='0' yoffset='2' xadvance='2' />
<char id='108' x='35' y='45' width='1' height='5' xoffset='0' yoffset='2' xadvance='2' />
<char id='33' x='37' y='45' width='1' height='5' xoffset='1' yoffset='2' xadvance='3' />
<char id='161' x='39' y='45' width='1' height='5' xoffset='0' yoffset='2' xadvance='3' />
<char id='166' x='41' y='45' width='1' height='5' xoffset='0' yoffset='2' xadvance='2' />
<char id='109' x='43' y='45' width='7' height='4' xoffset='0' yoffset='3' xadvance='8' />
<char id='119' x='51' y='45' width='7' height='4' xoffset='0' yoffset='3' xadvance='8' />
<char id='230' x='59' y='45' width='7' height='4' xoffset='0' yoffset='3' xadvance='8' />
<char id='97' x='67' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='99' x='72' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='101' x='77' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='110' x='82' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='111' x='87' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='115' x='92' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='117' x='97' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='118' x='102' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='120' x='107' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='122' x='112' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='215' x='117' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='248' x='122' y='45' width='4' height='4' xoffset='0' yoffset='3' xadvance='5' />
<char id='114' x='1' y='51' width='3' height='4' xoffset='0' yoffset='3' xadvance='4' />
<char id='178' x='5' y='51' width='3' height='4' xoffset='0' yoffset='2' xadvance='4' />
<char id='179' x='9' y='51' width='3' height='4' xoffset='0' yoffset='2' xadvance='4' />
<char id='185' x='13' y='51' width='1' height='4' xoffset='0' yoffset='2' xadvance='2' />
<char id='61' x='15' y='51' width='5' height='3' xoffset='0' yoffset='3' xadvance='6' />
<char id='171' x='21' y='51' width='5' height='3' xoffset='0' yoffset='3' xadvance='6' />
<char id='172' x='27' y='51' width='5' height='3' xoffset='0' yoffset='4' xadvance='6' />
<char id='187' x='33' y='51' width='5' height='3' xoffset='0' yoffset='3' xadvance='6' />
<char id='176' x='39' y='51' width='3' height='3' xoffset='0' yoffset='2' xadvance='4' />
<char id='44' x='43' y='51' width='2' height='3' xoffset='0' yoffset='6' xadvance='3' />
<char id='58' x='46' y='51' width='1' height='3' xoffset='1' yoffset='3' xadvance='4' />
<char id='94' x='48' y='51' width='4' height='2' xoffset='-1' yoffset='2' xadvance='4' />
<char id='126' x='53' y='51' width='4' height='2' xoffset='0' yoffset='3' xadvance='5' />
<char id='34' x='58' y='51' width='3' height='2' xoffset='0' yoffset='2' xadvance='4' />
<char id='96' x='62' y='51' width='2' height='2' xoffset='0' yoffset='2' xadvance='3' />
<char id='180' x='65' y='51' width='2' height='2' xoffset='0' yoffset='2' xadvance='3' />
<char id='184' x='68' y='51' width='2' height='2' xoffset='0' yoffset='7' xadvance='3' />
<char id='39' x='71' y='51' width='1' height='2' xoffset='0' yoffset='2' xadvance='2' />
<char id='95' x='73' y='51' width='5' height='1' xoffset='0' yoffset='7' xadvance='6' />
<char id='45' x='79' y='51' width='4' height='1' xoffset='0' yoffset='4' xadvance='5' />
<char id='173' x='84' y='51' width='4' height='1' xoffset='0' yoffset='4' xadvance='5' />
<char id='168' x='89' y='51' width='3' height='1' xoffset='1' yoffset='2' xadvance='5' />
<char id='175' x='93' y='51' width='3' height='1' xoffset='0' yoffset='2' xadvance='4' />
<char id='46' x='97' y='51' width='1' height='1' xoffset='0' yoffset='6' xadvance='2' />
<char id='183' x='99' y='51' width='1' height='1' xoffset='0' yoffset='4' xadvance='2' />
<char id='32' x='6' y='56' width='0' height='0' xoffset='0' yoffset='127' xadvance='3' />
</chars></font>";
    }
}

