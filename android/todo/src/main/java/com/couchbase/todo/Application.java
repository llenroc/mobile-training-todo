package com.couchbase.todo;

import android.content.Intent;
import android.os.Handler;
import android.widget.Toast;

import com.couchbase.lite.BasicAuthenticator;
import com.couchbase.lite.CouchbaseLiteException;
import com.couchbase.lite.Database;
import com.couchbase.lite.DatabaseConfiguration;
import com.couchbase.lite.Log;
import com.couchbase.lite.Replicator;
import com.couchbase.lite.ReplicatorChangeListener;
import com.couchbase.lite.ReplicatorConfiguration;

import java.net.URI;
import java.net.URISyntaxException;

public class Application extends android.app.Application implements ReplicatorChangeListener {

    private static final String TAG = Application.class.getSimpleName();

    private final static boolean LOGIN_FLOW_ENABLED = true;
    private final static boolean SYNC_ENABLED = true;

    private final static String DATABASE_NAME = "todo";
    private final static String SYNCGATEWAY_URL = "blip://10.0.2.2:4984/todo/";

    private Database database = null;
    private Replicator replicator;
    private String username = DATABASE_NAME;

    @Override
    public void onCreate() {
        super.onCreate();
        if (LOGIN_FLOW_ENABLED)
            showLoginUI();
        else
            startSession(DATABASE_NAME, null);
    }

    @Override
    public void onTerminate() {
        closeDatabase();
        super.onTerminate();
    }

    public Database getDatabase() {
        return database;
    }

    public String getUsername() {
        return username;
    }

    // -------------------------
    // Session/Login/Logout
    // -------------------------
    private void startSession(String username, String password) {
        openDatabase(username);
        this.username = username;
        startReplication(username, password);

        // TODO: After authenticated, move to next screen
        showApp();
    }

    // show loginUI
    private void showLoginUI() {
        startActivity(new Intent(getApplicationContext(), LoginActivity.class));
    }

    public void login(String username, String password) {
        this.username = username;
        startSession(username, password);
    }

    public void logout() {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                stopReplication();
                closeDatabase();
                Application.this.username = null;
                showLoginUI();
            }
        });
    }

    private void showApp() {
        startActivity(new Intent(getApplicationContext(), ListsActivity.class));
    }

    // -------------------------
    // Database operation
    // -------------------------

    private void openDatabase(String dbname) {
        DatabaseConfiguration config = new DatabaseConfiguration(getApplicationContext());
        try {
            database = new Database(dbname, config);
        } catch (CouchbaseLiteException e) {
            Log.e(TAG, "Failed to create Database instance: %s - %s", e, dbname, config);
            // TODO: error handling
        }
    }

    private void closeDatabase() {
        if (database != null) {
            try {
                database.close();
            } catch (CouchbaseLiteException e) {
                Log.e(TAG, "Failed to close Database", e);
                // TODO: error handling
            }
        }
    }

    private void createDatabaseIndex() {

    }

    // -------------------------
    // Replicator operation
    // -------------------------
    private void startReplication(String username, String password) {
        if (!SYNC_ENABLED) return;

        URI uri;
        try {
            uri = new URI(SYNCGATEWAY_URL);
        } catch (URISyntaxException e) {
            Log.e(TAG, "Failed parse URI: %s", e, SYNCGATEWAY_URL);
            return;
        }

        ReplicatorConfiguration config = new ReplicatorConfiguration(database, uri);
        config.setReplicatorType(ReplicatorConfiguration.ReplicatorType.PUSH_AND_PULL);
        config.setContinuous(true);

        // authentication
        if (username != null && password != null)
            config.setAuthenticator(new BasicAuthenticator(username, password));

        replicator = new Replicator(config);
        replicator.addChangeListener(this);
        replicator.start();
    }

    private void stopReplication() {
        if (!SYNC_ENABLED) return;

        replicator.stop();
    }

    private void runOnUiThread(Runnable runnable) {
        new Handler(getApplicationContext().getMainLooper()).post(runnable);
    }

    // --------------------------------------------------
    // ReplicatorChangeListener implementation
    // --------------------------------------------------
    @Override
    public void changed(Replicator replicator, Replicator.Status status, CouchbaseLiteException error) {
        Log.i(TAG, "[Todo] Replicator: status -> %s, error -> %s", status, error);
        if (error != null && error.getCode() == 401) {
            Toast.makeText(getApplicationContext(), "Authentication Error: Your username or password is not correct.", Toast.LENGTH_LONG).show();
            logout();
        }
    }
}