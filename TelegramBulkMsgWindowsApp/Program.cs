using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using TeleSharp.TL;
using TeleSharp.TL.Contacts;
using TLSharp.Core;

namespace TelegramBulkMsgWindowsApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var message =
                "مشتریان عزیز" +
                Environment.NewLine +
                "درود بر شما " +
                Environment.NewLine +
                "با احترام ضمن آرزوی سلامتی و بهروزی " +
                "بدینوسیله به اطلاع میرساند که شرکت پرنده باران پارس" +
                "با نام تجاری ABAone" +
                "فروشگاه اینترنتی خود را در لینک زیر فعال نموده است." +
                "موفق و پیروز باشید" +
                "انصاری" +
                Environment.NewLine +
                "تلفن واحد فروش :" +
                "02191013200" +
                Environment.NewLine +
                "موبایل واحد فروش :" +
                "09199122519" +
                Environment.NewLine +
                "https://abaone.ir/shop/" +
                Environment.NewLine +
                "اینستاگرام:" +
                "instagram: https://www.instagram.com/abaone.ir";

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
                    TLVector<TLInputPhoneContact> vectorInputPhoneContact = new TLVector<TLInputPhoneContact>();
                    vectorInputPhoneContact.Add(new TLInputPhoneContact
                    {
                        FirstName = line,
                        LastName = "",
                        Phone = line
                    });
                    TLImportedContacts importedContacts = await client.SendRequestAsync<TLImportedContacts>(new TLRequestImportContacts
                    {
                        Contacts = vectorInputPhoneContact
                    });

                    //send message
                    TLUser user = null;
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

                    //last seen status
                    var q = user.Status.GetType().ToString();

                    //remove from contacts
                    TLRequestDeleteContact req = new TLRequestDeleteContact
                    {
                        Id = new TLInputUser
                        {
                            UserId = user.Id,
                            AccessHash = (long)user.AccessHash
                        }
                    };
                    await client.SendRequestAsync<object>(req);

                    await updateLastPhoneAsync(line);
                }
            }

            Console.WriteLine("all jobs has been done!");
            Console.ReadLine();
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
