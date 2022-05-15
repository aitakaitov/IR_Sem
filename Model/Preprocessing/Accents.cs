using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Preprocessing
{
    /// <summary>
    /// Inspired by https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net
    /// and significantly sped up
    /// </summary>
    public static class Accents
    {
        private static string AreAccentedChars = "äæǽöœüÄÜÖÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶАàáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặаБбÇĆĈĊČçćĉċčДдÐĎĐΔðďđδÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭèéêëēĕėęěέεẽẻẹềếễểệеэФфĜĞĠĢΓГҐĝğġģγгґĤĦĥħ" +
            "ÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫìíîïĩīĭǐįıηήίιϊỉịиыїĴĵĶΚКķκкĹĻĽĿŁΛЛĺļľŀłλлМмÑŃŅŇΝНñńņňŉνнÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢОòóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợоПпŔŖŘΡРŕŗřρрŚŜŞȘŠΣСśŝşșšſσςсȚŢŤŦτТțţťŧт" +
            "ÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựуÝŸŶΥΎΫỲỸỶỴЙýÿŷỳỹỷỵйŴŵŹŻŽΖЗźżžζзƒπβμ";
        private static string NonAccentedChars = "aaaoouAUOAAAAAAAAAAAAAAAAAAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaaaaBbCCCCCcccccDdDDDDddddEEEEEEEEEEEEEEEEEEEEEeeeeeeeeeeeeeeeeeeeeeFfGGGGGGGgggggggHHhh" +
            "IIIIIIIIIIIIIIIIIIIiiiiiiiiiiiiiiiiiiiiJjKKKkkkLLLLLLLlllllllMmNNNNNNnnnnnnnOOOOOOOOOOOOOOOOOOOOOOOOOOOOoooooooooooooooooooooooooooooPpRRRRRrrrrrSSSSSSSsssssssssTTTTTTttttt" +
            "UUUUUUUUUUUUUUUUUUUUUUUUuuuuuuuuuuuuuuuuuuuuuuuuuuYYYYYYYYYYYyyyyyyyyWwZZZZZzzzzzfpvm";

        /// <summary>
        /// Removes accents from a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveAccents(string s)
        {
            StringBuilder text = new();

            foreach (char c in s)
            {
                int index = AreAccentedChars.IndexOf(c);

                if (index == -1)
                {
                    text.Append(c);
                }
                else
                {
                    text.Append(NonAccentedChars[index]);
                }
            }

            return text.ToString();
        }
    }
}
