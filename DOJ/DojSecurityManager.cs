using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DOJ
{
    public class DojSecurityManager
    {
        private const string Salt = "a716db47842a8aa435a1327e72ee8ce8";
        private const string Key = "6ee82dc8a2d9176ee20ba5c5804eb5b0";
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;
        private readonly UTF8Encoding _encoder;

        public DojSecurityManager()
        {
            var rm = new RijndaelManaged();
            var saltBytes = Encoding.ASCII.GetBytes(Salt);
            var deriver = new Rfc2898DeriveBytes(Key, saltBytes);
            var key = deriver.GetBytes(32);
            var vector = deriver.GetBytes(16);
            _encryptor = rm.CreateEncryptor(key, vector);
            _decryptor = rm.CreateDecryptor(key, vector);
            _encoder = new UTF8Encoding();
        }

        public string Encrypt(string plain)
        {
            var enc = Transform(_encoder.GetBytes(plain), _encryptor);
            return Convert.ToBase64String(enc);
        }

        public string Decrypt(string cipher)
        {
            var dec = Transform(Convert.FromBase64String(cipher), _decryptor);
            return _encoder.GetString(dec);

        }

        private static byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            var stream = new MemoryStream();
            using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }

        public List<User> ReadUsers()
        {
            var lines = File.ReadAllLines(GetUserbaseDir());
            if (lines.Length == 0 || (lines.Length == 1 && lines[0].Trim().Equals("")))
            {
                return new List<User>();
            }
            return lines.Select(Decrypt).Select(User.FromString).ToList();
        }

        public void WriteUsers(List<User> users)
        {
            using (var writer = new StreamWriter(new FileStream(GetUserbaseDir(), FileMode.Create)))
            {
                foreach (var cipher in users.Select(user => user.ToString()).Select(Encrypt))
                {
                    writer.WriteLine(cipher);
                }
            }
        }

        public static string GetUserbaseDir()
        {
            var dir = Directory.GetParent(Environment.CurrentDirectory);
            while (dir != null && !dir.FullName.EndsWith("\\DOJ"))
            {
                dir = dir.Parent;
            }
            var file = new FileInfo(dir?.FullName + "\\udb.tx");
            return file.Exists ? file.FullName : null;
        }
    }
}
