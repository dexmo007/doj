namespace DOJ
{
    public class User
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

        public string Name { get; set; }

        public const string FullView = "fullview";
        public const string Admin = "admin";

        public User() : this(string.Empty, string.Empty)
        {
        }

        public User(string name, string password)
        {
            Username = name;
            Password = password;
        }

        public bool IsValid()
        {
            // todo validate chars in username/ password
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !Username.Contains(" ") &&
                   !Password.Contains(" ");
        }

        public override string ToString()
        {
            return Username + "," + Password + "," + Role + "," + Name;
        }

        public static User FromString(string s)
        {
            // assert valid string
            var split = s.Split(',');
            var newUser = new User(split[0], split[1]);
            if (split.Length == 2)
            {
                return newUser;
            }
            newUser.Role = split[2];
            newUser.Name = split[3];
            return newUser;
        }

        public override bool Equals(object obj)
        {
            return obj is User && Username.Equals(((User)obj).Username);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() + 0;
        }
    }
}