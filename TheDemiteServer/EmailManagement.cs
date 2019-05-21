using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace TheDemiteServer
{
    class EmailManagement
    {
        public int SendEmail(string to, string resetPassword, int emailType)
        {
            int result = -1;

            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.UseDefaultCredentials = false;

                message.From = new MailAddress("thedemitegame@gmail.com");
                message.To.Add(to);
                if (emailType == 1)
                {
                    message.Subject = "The Demite - Reset Password ";
                    message.Body = "Hello. \n \nYou recently requested to reset your password for your The Demite Account. This is your new password :" + resetPassword + " \n \nIf you did not request a password reset, please ignore this email or replay to let us know. \n \nThanks, \n \nThe Demite Team ";
                }
                else
                {
                    if(emailType  == 2)
                    {
                        message.Subject = "The Demite - Change Password";
                        message.Body = "Hello. \n \nYour password was recently changed. If you feel that you did not changed your password. Please contact our team by sending an email to this address.\n \nIf you are the one who did this then you do not have to worry. \n \nBest regards \n \nThe Demite Team ";
                    }
                }

                smtp.Port = 587;
                smtp.Credentials = new System.Net.NetworkCredential("thedemitegame@gmail.com", "thedemite123");
                smtp.EnableSsl = true;

                smtp.Send(message);

                result = 1; 
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        public string CreateRandomPassword(int length)
        {
            string newRandomPassword = "";

            Random r = new Random();
            for (int i=0; i<length; i++)
            {
                int newInt = r.Next(10);
                newRandomPassword += newInt;
            }

            return newRandomPassword;
        }
    }
}
