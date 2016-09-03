namespace OasCommonLib.Data
{
    using System;
    using System.Text.RegularExpressions;

    public class AudioNote : CommonInfo
    {
        public long EnvelopeId { get; set; }

        private static string pattern = @"^.*?-(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})\..*";
        private static Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);

        public long DateTicks
        {
            get
            {
                long ticks = DateTime.Now.Ticks;
                int tmp;
                int[] data = new int[] { 0, 0, 0, 0, 0, 0 };
                string[] parts = rgx.Split(FileName);

                if (8 == parts.Length)
                {
                    for (int i = 1; i < parts.Length - 1; ++i)
                    {
                        if (int.TryParse(parts[i], out tmp))
                        {
                            data[i - 1] = tmp;
                        }
                        else
                        {
                            ticks = DateTime.Now.Ticks;
                            break;
                        }
                    }
                    DateTime dt = new DateTime(data[0], data[1], data[2], data[3], data[4], data[5]);
                    ticks = dt.Ticks;
                }

                return ticks;
            }
        }
        public string NoteName
        {
            get
            {
                string[] parts = FileName.Split('-');

                if (2 == parts.Length)
                {
                    parts = parts[1].Split('.');
                    if (2 == parts.Length)
                    {
                        return parts[0];
                    }
                }
                return FileName;
            }
        }

        public AudioNote() : base() { }
    }
}
