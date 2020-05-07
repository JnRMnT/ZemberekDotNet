using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Normalization.Tests.Deasciifier
{
    [TestClass]
    public class TestDeasciifier
    {
        private static string[] asciiStrings = {
      "Acimasizca acelya gorunen bir sacmaliktansa acilip sacilmak...",
      "Acisindan bagirip cagirarak sacma sozler soylemek.",
      "Bogurtuler opucukler.",
      "BUYUKCE BIR TOPAC TOPARLAGI VE DE YUMAGI yumagi.",
      "Bilgisayarlarda uc adet bellek turu bulunur. Islemci icerisinde yer alan yazmaclar, son derece hizli ancak cok sinirli hafizaya sahiptirler. Islemcinin cok daha yavas olan ana bellege olan erisim gereksinimini gidermek icin kullanilirlar. Ana bellek ise Rastgele erisimli bellek (REB veya RAM, Random Access Memory) ve Salt okunur bellek (SOB veya ROM, Read Only Memory) olmak uzere ikiye ayrilir. RAM'a istenildigi zaman yazilabilir ve icerigi ancak guc surdugu surece korunur. ROM'sa sâdece okunabilen ve onceden yerlestirilmis bilgiler icerir. Bu icerigi gucten bagimsiz olarak korur. Ornegin herhangi bir veri veya komut RAM'da bulunurken, bilgisayar donanimini duzenleyen BIOS ROM'da yer alir.",
      "1969 yilinda 15 yasindayken 1000 lira transfer parasi alarak Camialti Spor Kulubu'nde amator futbolcu oldu. Daha sonra IETT Spor Kulubu'nun amator futbolcusu oldu. 1976 yilinda, IETT sampiyon oldugunda, Erdogan da bu takimda oynamaktaydi. Erokspor Kulubunde de futbola devam etti ve 16 yillik futbol yasamini 12 Eylul 1980 Askeri Darbesi sonrasinda birakti ve daha fazla siyasi faaliyet..."};

        private static string[] turkishStrings = {
      "Acımasızca açelya görünen bir saçmalıktansa açılıp saçılmak...",
      "Acısından bağırıp çağırarak saçma sözler söylemek.",
      "Böğürtüler öpücükler.",
      "BÜYÜKÇE BİR TOPAÇ TOPARLAĞI VE DE YUMAĞI yumağı.",
      "Bilgisayarlarda üç adet bellek turu bulunur. İşlemci içerisinde yer alan yazmaçlar, son derece hızlı ancak çok sınırlı hafızaya sahiptirler. İşlemcinin çok daha yavaş olan ana bellege olan erişim gereksinimini gidermek için kullanılırlar. Ana bellek ise Rastgele erişimli bellek (REB veya RAM, Random Access Memory) ve Salt okunur bellek (SOB veya ROM, Read Only Memory) olmak üzere ikiye ayrılır. RAM'a istenildiği zaman yazılabilir ve içeriği ancak güç sürdüğü sürece korunur. ROM'sa sâdece okunabilen ve önceden yerleştirilmiş bilgiler içerir. Bu içeriği güçten bağımsız olarak korur. Örneğin herhangi bir veri veya komut RAM'da bulunurken, bilgisayar donanımını düzenleyen BİOS ROM'da yer alır.",
      "1969 yılında 15 yaşındayken 1000 lira transfer parası alarak Camialtı Spor Kulübü'nde amatör futbolcu oldu. Daha sonra İETT Spor Kulübü'nün amatör futbolcusu oldu. 1976 yılında, İETT şampiyon olduğunda, Erdoğan da bu takımda oynamaktaydı. Erokspor Kulübünde de futbola devam etti ve 16 yıllık futbol yaşamını 12 Eylül 1980 Askeri Darbesi sonrasında bıraktı ve daha fazla siyasi faaliyet..."};

        [TestMethod]
        public void BasicTest()
        {
            for (int i = 0; i < asciiStrings.Length; i++)
            {
                Assert.AreEqual(turkishStrings[i], new Normalization.Deasciifier.Deasciifier(asciiStrings[i]).ConvertToTurkish(), ("Test " + (i + 1)));
            }
        }
    }
}
