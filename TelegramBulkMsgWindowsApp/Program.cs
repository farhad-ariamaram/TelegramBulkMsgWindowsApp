using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using TelegramBulkMsgWindowsApp.Models;
using TeleSharp.TL;
using TeleSharp.TL.Contacts;
using TLSharp.Core;

namespace TelegramBulkMsgWindowsApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TelegramBulkMsgDBContext _db = new TelegramBulkMsgDBContext();
            
            try
            {
                var message =
                "مشتریان عزیز" +
                Environment.NewLine +
                Environment.NewLine +
                "درود بر شما " +
                Environment.NewLine +
                Environment.NewLine +
                "با احترام ضمن آرزوی سلامتی و بهروزی " +
                "بدینوسیله به اطلاع میرساند که شرکت پرنده باران پارس" +
                "با نام تجاری ABAone" +
                "فروشگاه اینترنتی خود را در لینک زیر فعال نموده است." +
                "موفق و پیروز باشید" +
                Environment.NewLine +
                "انصاری" +
                Environment.NewLine +
                Environment.NewLine +
                "تلفن واحد فروش :" +
                Environment.NewLine +
                Environment.NewLine +
                "02191013200" +
                Environment.NewLine +
                Environment.NewLine +
                "موبایل واحد فروش :" +
                Environment.NewLine +
                Environment.NewLine +
                "09199122519" +
                Environment.NewLine +
                Environment.NewLine +
                "https://abaone.ir/shop/" +
                Environment.NewLine +
                Environment.NewLine +
                "اینستاگرام:" +
                Environment.NewLine +
                Environment.NewLine +
                "instagram: https://www.instagram.com/abaone.ir";

                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

                var client = new TelegramClient(6249861, "cf32c431c0d8402a2856c8046aa9d0ac");
                await client.ConnectAsync();
                if (!client.IsUserAuthorized())
                {
                    var hash = await client.SendCodeRequestAsync("+989199122519");
                    Console.WriteLine("enter code: ");
                    var code = Console.ReadLine();

                    await client.MakeAuthAsync("+989199122519", hash, code);
                }

                using (FileStream fs = File.Open("phones.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                using (StreamReader sr = new StreamReader(bs))
                {
                    string line;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        line = "+98" + line.Substring(1);
                        Console.WriteLine($"work on: {line}");

                        //add to contacts
                        TLImportedContacts importedContacts = null;
                        try
                        {
                            TLVector<TLInputPhoneContact> vectorInputPhoneContact = new TLVector<TLInputPhoneContact>();
                            vectorInputPhoneContact.Add(new TLInputPhoneContact
                            {
                                FirstName = line,
                                LastName = "",
                                Phone = line
                            });
                            importedContacts = await client.SendRequestAsync<TLImportedContacts>(new TLRequestImportContacts
                            {
                                Contacts = vectorInputPhoneContact
                            });
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"add {line} to contacts failed!");
                        }


                        //send message
                        TLUser user = null;
                        try
                        {
                            if (importedContacts.Users.Count > 0)
                            {
                                //telegram found :)
                                user = importedContacts.Users
                                    .Cast<TLUser>()
                                    .SingleOrDefault();
                                await client.SendMessageAsync(new TLInputPeerUser() { UserId = user.Id }, message);

                            }
                            else
                            {
                                //current phone has no telegram :(
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"send msg to {line} failed!");
                        }
                        


                        //last seen status
                        string lastSeen = "";
                        try
                        {
                            if (user!=null)
                            {
                                TLUserStatusOffline q = (TLUserStatusOffline)user.Status;
                                var w = q.WasOnline;
                                dtDateTime = dtDateTime.AddSeconds(w).ToLocalTime();
                                lastSeen = dtDateTime.ToString();
                            }
                        }
                        catch (Exception)
                        {
                            try
                            {
                                if (user != null)
                                {
                                    var q = user.Status.GetType();
                                    lastSeen = q.ToString();
                                }
                            }
                            catch (Exception)
                            {
                                lastSeen = "";
                            }
                        }



                        //add to db
                        Person person = new Person()
                        {
                            Phone = line,
                            LastSeen = lastSeen,
                            SendDate = DateTime.Now,
                            HasTelegram = !(lastSeen=="")
                        };
                        await _db.AddAsync(person);
                        await _db.SaveChangesAsync();


                        //remove from contacts
                        try
                        {
                            if (user != null)
                            {
                                TLRequestDeleteContact req = new TLRequestDeleteContact
                                {
                                    Id = new TLInputUser
                                    {
                                        UserId = user.Id,
                                        AccessHash = (long)user.AccessHash
                                    }
                                };
                                await client.SendRequestAsync<object>(req);
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"remove contact {line} failed!");
                        }
                        

                        await updateLastPhoneAsync(line);
                    }
                }

                Console.WriteLine("all phones has been done!");
                Console.ReadLine();
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

            
        }

        private async static Task updateLastPhoneAsync(string text)
        {
            using (StreamWriter writetext = new StreamWriter("lastPhone.txt", false))
            {
                await writetext.WriteLineAsync(text);
            }
        }

    }
}
