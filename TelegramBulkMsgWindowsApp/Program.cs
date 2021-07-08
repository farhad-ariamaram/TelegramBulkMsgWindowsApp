using System;
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
        static int i = 1;
        static async Task Main(string[] args)
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Start();

            var client = new TelegramClient(6249861, "cf32c431c0d8402a2856c8046aa9d0ac");
            await client.ConnectAsync();
            if (!client.IsUserAuthorized())
            {
                var hash = await client.SendCodeRequestAsync("+989199122519");
                Console.WriteLine("enter code: ");
                var code = Console.ReadLine();

                await client.MakeAuthAsync("+989199122519", hash, code);
            }

            TLVector<TLInputPhoneContact> vectorInputPhoneContact = new TLVector<TLInputPhoneContact>();
            vectorInputPhoneContact.Add(new TLInputPhoneContact
            {
                FirstName = "فرهاد",
                LastName = "آریامرام",
                Phone = "+989359313137"
            });

            TLImportedContacts importedContacts = await client.SendRequestAsync<TLImportedContacts>(new TLRequestImportContacts
            {
                Contacts = vectorInputPhoneContact
            });


            TLUser user = null;
            if (importedContacts.Users.Count > 0)
            {
                user = importedContacts.Users
                    .Cast<TLUser>()
                    .SingleOrDefault();
                await client.SendMessageAsync(new TLInputPeerUser() { UserId = user.Id }, "salam...");
            }

            TLRequestDeleteContact req = new TLRequestDeleteContact
            {
                Id = new TLInputUser
                {
                    UserId = user.Id,
                    AccessHash = (long)user.AccessHash
                }
            };
            await client.SendRequestAsync<object>(req);


            aTimer.Stop();
            Console.ReadLine();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine($"{i}");
            i++;
        }
    }
}
