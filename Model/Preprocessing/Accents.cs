using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Preprocessing
{
    /// <summary>
    /// Taken from https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net
    /// and slightly modified
    /// </summary>
    public static class Accents
    {
        private static string AreAccentedChars = "äæǽöœüÄÜÖÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶАàáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặаБбÇĆĈĊČçćĉċčДдÐĎĐΔðďđδÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭèéêëēĕėęěέεẽẻẹềếễểệеэФфĜĞĠĢΓГҐĝğġģγгґĤĦĥħ" +
            "ÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫìíîïĩīĭǐįıηήίιϊỉịиыїĴĵĶΚКķκкĹĻĽĿŁΛЛĺļľŀłλлМмÑŃŅŇΝНñńņňŉνнÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢОòóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợоПпŔŖŘΡРŕŗřρрŚŜŞȘŠΣСśŝşșšſσςсȚŢŤŦτТțţťŧт" +
            "ÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựуÝŸŶΥΎΫỲỸỶỴЙýÿŷỳỹỷỵйŴŵŹŻŽΖЗźżžζзƒπβμ";
        private static string NonAccentedChars = "aaaoouAUOAAAAAAAAAAAAAAAAAAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaaaaBbCCCCCcccccDdDDDDddddEEEEEEEEEEEEEEEEEEEEEeeeeeeeeeeeeeeeeeeeeeFfGGGGGGGgggggggHHhh" +
            "IIIIIIIIIIIIIIIIIIIiiiiiiiiiiiiiiiiiiiiJjKKKkkkLLLLLLLlllllllMmNNNNNNnnnnnnnOOOOOOOOOOOOOOOOOOOOOOOOOOOOoooooooooooooooooooooooooooooPpRRRRRrrrrrSSSSSSSsssssssssTTTTTTttttt" +
            "UUUUUUUUUUUUUUUUUUUUUUUUuuuuuuuuuuuuuuuuuuuuuuuuuuYYYYYYYYYYYyyyyyyyyWwZZZZZzzzzzfpvm";

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
