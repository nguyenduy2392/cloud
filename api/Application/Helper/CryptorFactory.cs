using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Application.Options;
using System.Text;

namespace Application.Helper
{
    public interface ICryptorFactory
    {
        /// <summary>
        /// Tạo chuỗi mã hóa
        /// </summary>
        /// <param name="password">Mật khẩu</param>
        /// <returns></returns>
        string ToHashPassword(string password);
    }

    public class CryptorFactory : ICryptorFactory
    {
        private readonly AppSetting _option;
        public CryptorFactory(IOptions<AppSetting> options)
        {
            _option = options.Value;
        }

        /// <summary>
        /// Tạo chuỗi mã hóa
        /// </summary>
        /// <param name="password">Mật khẩu</param>
        /// <returns></returns>
        public string ToHashPassword(string password)
        {
            var hashed = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.UTF8.GetBytes(_option.Secret),
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                    );

            return Convert.ToBase64String(hashed);
        }
    }
}
