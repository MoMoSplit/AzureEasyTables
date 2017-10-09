/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342
 */
//#define OFFLINE_SYNC_ENABLED

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.IO;

namespace MoMoChat
{
    public partial class Manager
    {
        static Manager defaultInstance = new Manager();
        MobileServiceClient client;


        IMobileServiceSyncTable<Group> groupTable;
        IMobileServiceSyncTable<Message> messageTable;


        const string offlineDbPath = @"momoChat.db";
        private bool isInitialized;

        private Manager()
        {
            this.client = new MobileServiceClient(Constants.ApplicationURL);
            
            

        }

        public async Task Initialize()
        {
            if (isInitialized)
                return;

            var store = new MobileServiceSQLiteStore(offlineDbPath);
            store.DefineTable<Group>();
            store.DefineTable<Message>();

            //Initializes the SyncContext using the default IMobileServiceSyncHandler.
            await this.client.SyncContext.InitializeAsync(store);

            this.groupTable = client.GetSyncTable<Group>();
            this.messageTable = client.GetSyncTable<Message>();

            isInitialized = true;
        }

        public static Manager DefaultManager
        {
            get
            {
                return defaultInstance;
            }
            private set
            {
                defaultInstance = value;
            }
        }

        public MobileServiceClient CurrentClient
        {
            get { return client; }
        }

        public async Task<ObservableCollection<Group>> GetGroupsAsync(bool syncItems = false)
        {
            try
            {
                if (syncItems)
                {
                    await this.SyncAsync();
                }

                IEnumerable<Group> items = await groupTable.ToEnumerableAsync();

                return new ObservableCollection<Group>(items);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"Invalid sync operation: {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }

        public async Task<ObservableCollection<Message>> GetMessageAsync(string groupId, bool syncItems = false)
        {
            try
            {
                if (syncItems)
                {
                    await this.SyncAsync();
                }

                IEnumerable<Message> items = await messageTable.Where(x => x.GroupId == groupId).ToEnumerableAsync();

                return new ObservableCollection<Message>(items);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"Invalid sync operation: {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }

        public async Task SaveGroupAsync(Group item)
        {
            if (item.Id == null)
            {
                await groupTable.InsertAsync(item);
            }
            else
            {
                await groupTable.UpdateAsync(item);
            }
        }

        public async Task SaveMessageAsync(Message item)
        {
            if (item.Id == null)
            {
                await messageTable.InsertAsync(item);
            }
            else
            {
                await messageTable.UpdateAsync(item);
            }
        }

        public async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                await this.client.SyncContext.PushAsync();

                await this.groupTable.PullAsync(
                    //The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
                    //Use a different query name for each unique query in your program
                    "allGroups",
                    this.groupTable.CreateQuery());


                await this.messageTable.PullAsync(
                    //The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
                    //Use a different query name for each unique query in your program
                    "allMessages",
                    this.messageTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }

                    Debug.WriteLine(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
            }
        }
    }
}
