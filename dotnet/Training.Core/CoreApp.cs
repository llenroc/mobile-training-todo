﻿//
// CoreApp.cs
//
// Author:
// 	Jim Borden  <jim.borden@couchbase.com>
//
// Copyright (c) 2016 Couchbase, Inc All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Acr.UserDialogs;
using Couchbase.Lite;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;
using XLabs.Platform.Device;
using XLabs.Platform.Services.Media;

namespace Training.Core
{
    /// <summary>
    /// This is the first location to be reached in the actual shared application
    /// </summary>
    public sealed class CoreApp : MvxApplication
    {
        #region Constants

        private static readonly Uri SyncGatewayUrl = new Uri("http://localhost:4984/todo/");

        #endregion

        #region Variables

        //private static Replication _pusher;
        //private static Replication _puller;
        private static Exception _syncError;
        //private static LiveQuery _conflictsLiveQuery;
        private static HashSet<IDocument> _accessDocuments = new HashSet<IDocument>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the database for the current session
        /// </summary>
        public static IDatabase Database { get; private set; }

        internal static CoreAppStartHint Hint { get; set; }

        #endregion

        #region Public API

        /// <summary>
        /// Starts a new session for a user
        /// </summary>
        /// <param name="username">The username to use for the session</param>
        /// <param name="password">The password to use for the session (optional)</param>
        /// <param name="newPassword">The new password for the database (optional)</param>
        public static void StartSession(string username, string password, string newPassword)
        {
            //if(Hint.UsePrebuiltDB) {
            //    InstallPrebuiltDB();
            //}

            var p = Hint.EncryptionEnabled ? password : null;
            var np = Hint.EncryptionEnabled ? newPassword : null;
            OpenDatabase(username, p, np);

            //if(Hint.SyncEnabled) {
            //    StartReplication(username, newPassword ?? password);
            //}
        }

        /// <summary>
        /// Installs a premade database for use in the application
        /// </summary>
        //public static void InstallPrebuiltDB()
        //{
        //    // TRAINING: Install pre-built database
        //    var db = AppWideManager.GetExistingDatabase("todo");
        //    if(db == null) {
        //        try {
        //            using(var asset = typeof(CoreApp).Assembly.GetManifestResourceStream("todo.zip")) {
        //                AppWideManager.ReplaceDatabase("todo", asset, false);
        //            }
        //        } catch(Exception e) {
        //            Debug.WriteLine($"Cannot replicate the database: {e}");
        //        }
        //    }
        //}

        /// <summary>
        /// Opens a given database by name for the session
        /// </summary>
        /// <param name="dbName">The name of the database to open</param>
        /// <param name="key">The key for the database (i.e. password, optional)</param>
        /// <param name="newKey">The updated key for the database (optional)</param>
        public static void OpenDatabase(string dbName, string key, string newKey)
        {
            // TRAINING: Create a database
            //var encryptionKey = default(SymmetricKey);
            //if(key != null) {
            //    encryptionKey = new SymmetricKey(key);
            //}

            Database = DatabaseFactory.Create(dbName);
            //if(newKey != null) {
            //    Database.ChangeEncryptionKey(new SymmetricKey(newKey));
            //}

            //Database.Changed += (sender, args) =>
            //{
            //    if (!args.External)
            //    {
            //        return;
            //    }

            //    var db = (IDatabase)sender;
            //    foreach(var change in args.DocIDs)
            //    {
            //        var changedDoc = db[change];
            //        if (!NeedsMonitoringFor(changedDoc.Properties, db.Name)) {
            //            continue;
            //        }

            //        _accessDocuments.Add(changedDoc);
            //        changedDoc.Saved += HandleAccessChanged;
            //    }
            //};
        }

        /// <summary>
        /// Closes the session database
        /// </summary>
        public static void CloseDatabase()
        {
            try {
                Database.Close();
            } catch(Exception e) {
                Debug.WriteLine($"Failed to close DB {e}");
            }
        }

        /// <summary>
        /// Starts a replication for the session
        /// </summary>
        /// <param name="username">The username to use for the replication</param>
        /// <param name="password">The password to use for replication auth (optional)</param>
        //public static void StartReplication(string username, string password = null)
        //{
        //    var authenticator = default(IAuthenticator);
        //    if(username != null && password != null) {
        //        authenticator = AuthenticatorFactory.CreateBasicAuthenticator(username, password);
        //    }

        //    var db = AppWideManager.GetDatabase(username);
        //    var pusher = db.CreatePushReplication(SyncGatewayUrl);
        //    pusher.Continuous = true;
        //    pusher.Authenticator = authenticator;
        //    pusher.Changed += HandleReplicationChanged;
            

        //    var puller = db.CreatePullReplication(SyncGatewayUrl);
        //    puller.Continuous = true;
        //    puller.Authenticator = authenticator;
        //    puller.Changed += HandleReplicationChanged;

        //    pusher.Start();
        //    puller.Start();

        //    _pusher = pusher;
        //    _puller = puller;
        //}

        /// <summary>
        /// Stops the session replication
        /// </summary>
        //public static void StopReplication()
        //{
        //    var pusher = Interlocked.Exchange(ref _pusher, null);
        //    var puller = Interlocked.Exchange(ref _puller, null);

        //    if(pusher != null) {
        //        pusher.Changed -= HandleReplicationChanged;
        //        pusher.Stop();
        //    }

        //    if(puller != null) {
        //        puller.Changed -= HandleReplicationChanged;
        //        puller.Stop();
        //    }
        //}

        #endregion

        #region Private API

        private static bool NeedsMonitoringFor(IDictionary<string, object> userAccessDoc, string user)
        {
            object docType;
            if(!userAccessDoc.TryGetValue("type", out docType)) {
                return false;
            }

            if((docType as string) != "task-list.user") {
                return false;
            }

            if(userAccessDoc["username"] as string != user) {
                return false;
            }

            return true;
        }

        private static T Lookup<T>(IDictionary<string, object> dic, string key) where T : class
        {
            object val;
            if(dic.TryGetValue(key, out val)) {
                return val as T;
            }

            return null;
        }

        private static T? LookupNullable<T>(IDictionary<string, object> dic, string key) where T : struct
        {
            object val;
            if(dic.TryGetValue(key, out val)) {
                return val as T?;
            }

            return null;
        }

        #endregion

        #region Overrides

        public override void Initialize()
        {
            CreatableTypes()
            .EndingWith("ViewModel")
            .AsTypes()
            .RegisterAsDynamic();

            Mvx.RegisterSingleton<IUserDialogs>(() => UserDialogs.Instance);
            Mvx.RegisterType<IMediaPicker>(() => Mvx.Resolve<IDevice>().MediaPicker);
            RegisterAppStart<TaskListsViewModel>();
        }

        #endregion

    }

    /// <summary>
    /// A custom start logic class for MvvmCross
    /// </summary>
    public sealed class CoreAppStart : MvxNavigatingObject, IMvxAppStart
    {

        #region Public API

        /// <summary>
        /// Creates the hint for starting CoreApp, which will control the way the app behaves
        /// </summary>
        /// <returns>The hint object</returns>
        public static CoreAppStartHint CreateHint()
        {
            var retVal = new CoreAppStartHint {
                LoginEnabled = false,
                EncryptionEnabled = false,
                SyncEnabled = false,
                UsePrebuiltDB = false,
                Username = "todo"
            };

            return retVal;
        }

        /// <summary>
        /// Starts the app
        /// </summary>
        /// <param name="hint">The hint object to use (See <see cref="CoreAppStart"/>) </param>
        public void Start(object hint = null)
        {
            CoreApp.Hint = (CoreAppStartHint)hint;
            if(CoreApp.Hint.LoginEnabled) {
                ShowViewModel<LoginViewModel>();
            } else {
                CoreApp.StartSession(CoreApp.Hint.Username, null, null);
                ShowViewModel<TaskListsViewModel>(new { loginEnabled = false });
            }

            
        }

        #endregion

    }

    /// <summary>
    /// The hints for how the application should function
    /// </summary>
    public sealed class CoreAppStartHint
    {
        /// <summary>
        /// Gets or sets whether or not to use login functionality
        /// </summary>
        public bool LoginEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether or not to use encryption on the local DB files
        /// </summary>
        public bool EncryptionEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether or not to use sync
        /// </summary>
        public bool SyncEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether or not to seed the app with a prepopulated database
        /// </summary>
        public bool UsePrebuiltDB { get; set; }

        /// <summary>
        /// Gets or sets the username to use for the session
        /// </summary>
        public string Username { get; set; }
    }
}

