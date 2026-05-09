using DontMissPassword.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Infrastructure.Implemention
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeKit.MimeMessage();
            email.From.Add(new MailboxAddress(
                _configuration.GetValue<string>("Email:SenderName"),
                _configuration.GetValue<string>("Email:SenderEmail")
                ));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtp.ConnectAsync(
                    _configuration.GetValue<string>("Email:SmtpServer"),
                    _configuration.GetValue<int>("Email:SmtpPort"),
                    MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(
                    _configuration.GetValue<string>("Email:Username"),
                    _configuration.GetValue<string>("Email:Password"));
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error sending email: " + ex.Message, ex);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
        public async Task SendOtpAsync(string email, string otp)
        {
            try
            {
                var subject = "Your OTP Code";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>OTP Verification - Dark Mode</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", sans-serif; background-color: #121212;'>
    <div style='background-color: #121212; padding: 20px; min-height: 100vh;'>
        <div style='max-width: 600px; margin: 0 auto; background-color: #1e1e1e; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);'>
            
            <!-- Header (Chuyển từ Cam sang Dark Gradient) -->
            <div style='background: linear-gradient(135deg, #2c2c2c 0%, #000000 100%); padding: 40px 20px; text-align: center; border-bottom: 1px solid #333333;'>
                <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 600; letter-spacing: 1px;'>DontMissPassword</h1>
                <p style='color: #bbbbbb; margin: 8px 0 0 0; font-size: 14px;'>Xác Minh Tài Khoản</p>
            </div>

            <!-- Content -->
            <div style='padding: 40px 30px; text-align: center;'>
                <h2 style='color: #ffffff; margin: 0 0 16px 0; font-size: 20px; font-weight: 600;'>Mã Xác Minh OTP</h2>
                
                <p style='color: #aaaaaa; font-size: 15px; line-height: 1.6; margin: 0 0 24px 0;'>
                    Chào mừng bạn đến với <strong style='color: #ffffff;'>DontMissPassword</strong>!<br/>
                    Vui lòng sử dụng mã dưới đây để hoàn tất xác minh tài khoản của bạn.
                </p>

                <p style='color: #777777; font-size: 13px; margin: 0 0 30px 0;'>Mã này có hiệu lực trong <strong>5 phút</strong></p>

                <!-- OTP Code Box (Chuyển sang nền tối viền nổi bật) -->
                <div style='background-color: #2d2d2d; border: 1px solid #444444; border-radius: 8px; padding: 20px; margin: 0 0 30px 0;'>
                    <code style='font-family: ""Monaco"", ""Courier New"", monospace; font-size: 40px; font-weight: 700; color: #00ff88; letter-spacing: 6px;'>{otp}</code>
                </div>

                <p style='color: #777777; font-size: 13px; line-height: 1.6; margin: 0;'>
                    Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này hoặc liên hệ với bộ phận hỗ trợ.
                </p>
            </div>

            <!-- Security Warning (Chỉnh màu cảnh báo trầm hơn) -->
            <div style='background-color: #2a2617; border-left: 4px solid #ffc107; padding: 15px 20px; margin: 0 30px 30px 30px; border-radius: 4px;'>
                <p style='color: #ffe082; font-size: 12px; margin: 0;'>
                    ⚠️ <strong>Lưu ý bảo mật:</strong> Không chia sẻ mã OTP này với bất kỳ ai. DontMissPassword sẽ không bao giờ yêu cầu mã này qua email.
                </p>
            </div>

            <!-- Footer -->
            <div style='background-color: #1a1a1a; border-top: 1px solid #333333; padding: 20px; text-align: center;'>
                <p style='color: #666666; font-size: 12px; margin: 0 0 8px 0;'>
                    &copy; 2026 DontMissPassword. Tất cả các quyền được bảo lưu.
                </p>
                <p style='color: #444444; font-size: 11px; margin: 0;'>
                    Đây là tin nhắn tự động, vui lòng không trả lời email này.
                </p>
            </div>
        </div>
    </div>
</body>
</html>
";
                await SendEmailAsync(email, subject, body);

            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error sending OTP email: " + ex.Message, ex);
            }
        }
    }
}
