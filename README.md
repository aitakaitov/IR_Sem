# Spuštění

Aplikaci je možné spustit pouze na OS Windows protože pro GUI používá WPF. Zároveň je potřeba mít .NET 6 + nainstalované všechny NuGet balíčky.

# Data

## Vlastní data

Data z prvního cvičení lze stáhnout na https://drive.google.com/file/d/1L15LbTGcChte-9UtB0IFkx4YL9mccL6H/view?usp=sharing. Jedná se o zip archiv se složkami <code>json-final</code> a <code>output-final</code>. V <code>json-final</code> jsou neupravená nacrawlovaná data, v <code>output-final</code> jsou data určěná k indexaci (jedná se o JSONy převedené do plaintextu, protože aplikace umožňuje zaindexovat pouze textové soubory). Pro zaindexování dat v GUI stačí vytvořit nový index a při výběru složky s daty vybrat složku <code>output-final</code>.

## TREC data

TREC data v JSON formátu jsou k dispozici na https://drive.google.com/file/d/1pt1xltkEjF9R7Jn0VAAsUAHLadrBTYy2/view?usp=sharing. Jedná se o data z CW převedená do JSONu. Archiv obsahuje složku <code>trec</code>, která obsahuje složky <code>topics</code> a <code>documents</code>. Při evaluaci v GUI stačí při výběru složky s daty vybrat složku <code>trec</code>. Způsob předzpracování je pro evaluaci fixně daný a nelze ho v GUI upravovat. Výstup evaluace se uloží do souboru <code>results.txt</code> ve složce <code>trec</code>. 

# Soubor s výsledky

Výsledky evaluačního skriptu jsou v souboru <code>results.txt</code> v kořenovém adresáři tohoto archivu.