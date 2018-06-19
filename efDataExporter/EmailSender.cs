using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;
using System.IO;
using efDataExtporter.DTO;

namespace efDataExtporter
{
    public class EmailSender
    {
        //logger object
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
    (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public readonly string _From_Address;

        private SmtpClient _mailServer;

        /// <summary>
        /// constructor
        /// </summary>
        public EmailSender()
        {
            try
            {
                //get mail client
                _mailServer = GetSmtpClient();

                if (ConfigurationManager.AppSettings["SmtpFromAddress"] != null)
                {
                    _From_Address = ConfigurationManager.AppSettings["SmtpFromAddress"].ToString();
                }
                else
                {
                    _From_Address = "noreply@gmail.com";
                }
            }
            catch(Exception ex)
            {
                _log.Error(ex.Message);
            }
            
        }

        /// <summary>
        /// send email to recipient
        /// </summary>
        /// <param name="xRecipient">notification recipient</param>
        /// <param name="xEmailSubject">email subject</param>
        /// <param name="xEmailBody">email content</param>
        /// <param name="xAttachmentFullPath">(optional) attachment full file path</param>
        public void Send(string xRecipient, string xEmailSubject, string xEmailBody, string xAttachmentFullPath = null)
        {
            if(_mailServer != null)
            {
                //only send email if not running as demo
                //or running as demo and email contains the wndirect domain
                if (!IsRunningAsDemo())
                {
                    //create MailMessage object
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.Subject = xEmailSubject;
                    mailMessage.Body = xEmailBody;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.From = new MailAddress(_From_Address);

                    //add attachment if exists
                    if(xAttachmentFullPath != null)
                    {
                        if((File.Exists(xAttachmentFullPath)))
                        {
                            Attachment attachment = new Attachment(xAttachmentFullPath);
                            mailMessage.Attachments.Add(attachment);
                        }
                        else
                        {
                            _log.Error("Unable to attach file as it has not been found in: " + xAttachmentFullPath);
                        }
                        
                    }

                    //create MailAddress
                    if (xRecipient.Contains(","))
                    {
                        string[] recipients = xRecipient.Split(',');
                        foreach (string r in recipients)
                        {
                            mailMessage.To.Add(r);
                        }
                    }
                    else
                    {
                        MailAddress recipientEmail = new MailAddress(xRecipient, xRecipient);
                        mailMessage.To.Add(recipientEmail);
                    }
                    
                    //send message
                    _mailServer.Send(mailMessage);
                }
            }
        }

        /// <summary>
        /// determines whether application is running in demo mode
        /// </summary>
        /// <returns>true if set to DEMO, otherwise false</returns>
        private bool IsRunningAsDemo()
        {
            bool IsRunningAsDemo = false;

            try
            {
                string IsRunningAsDemoValue = ConfigurationManager.AppSettings["RunMode"].ToString();

                if (IsRunningAsDemoValue == "DEMO")
                {
                    IsRunningAsDemo = true;
                }
            }
            catch (Exception)
            {
                _log.Info("IsRunningAsDemo: configuration item RunMode not found so running as LIVE");
            }
            return IsRunningAsDemo;
        }

        /// <summary>
        /// get smtp client using config file
        /// </summary>
        /// <returns>smtp client or null</returns>
        protected SmtpClient GetSmtpClient()
        {
            SmtpClient smtpClient = null;
            try
            {
                SmtpClient smtpClientTry = new SmtpClient();

                smtpClientTry.Host = ConfigurationManager.AppSettings["SmtpHost"].ToString();
                smtpClientTry.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"].ToString());

                string smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"].ToString();
                string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"].ToString();

                smtpClientTry.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);

                if ((ConfigurationManager.AppSettings["SmtpUseSSL"] != null) && (ConfigurationManager.AppSettings["SmtpUseSSL"].ToString().ToLower() == "true"))
                {
                    smtpClientTry.EnableSsl = true;
                }
                else
                {
                    smtpClientTry.EnableSsl = false;
                }
                
                smtpClient = smtpClientTry;

            }
            catch (Exception)
            {
                //do nothing
            }

            return smtpClient;
        }
    }
}
