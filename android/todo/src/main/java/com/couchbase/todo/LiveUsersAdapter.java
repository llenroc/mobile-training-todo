package com.couchbase.todo;

import android.support.annotation.NonNull;
import android.support.annotation.Nullable;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

import com.couchbase.lite.DataSource;
import com.couchbase.lite.Database;
import com.couchbase.lite.Document;
import com.couchbase.lite.Expression;
import com.couchbase.lite.LiveQuery;
import com.couchbase.lite.LiveQueryChange;
import com.couchbase.lite.LiveQueryChangeListener;
import com.couchbase.lite.Query;
import com.couchbase.lite.Result;
import com.couchbase.lite.ResultSet;
import com.couchbase.lite.SelectResult;

/**
 * Created by hideki on 6/26/17.
 */

public class LiveUsersAdapter  extends ArrayAdapter<String> {
    private static final String TAG = LiveUsersAdapter.class.getSimpleName();

    private UsersFragment fragment;
    private Database db;
    private String listID;

    private LiveQuery query;

    public LiveUsersAdapter(UsersFragment fragment, Database db, String listID) {
        super(fragment.getContext(), 0);
        this.fragment = fragment;
        this.db = db;
        this.listID = listID;

        this.query = query();
        this.query.addChangeListener(new LiveQueryChangeListener() {
            @Override
            public void changed(LiveQueryChange change) {
                clear();
                ResultSet rs = change.getRows();
                Result result;
                while ((result = rs.next()) != null) {
                    String id = result.getString(0);
                    add(result.getString(0));
                }
                notifyDataSetChanged();
            }
        });
        this.query.run();
    }

    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        if (convertView == null)
            convertView = LayoutInflater.from(getContext()).inflate(R.layout.view_user, parent, false);

        String id = getItem(position);
        final Document user = db.getDocument(id);
        if (user == null)
            return convertView;

        // text
        TextView userText = convertView.findViewById(R.id.user_name);
        userText.setText(user.getString("username"));

        return convertView;
    }

    private LiveQuery query() {
        return Query.select(SelectResult.expression(Expression.meta().getId()))
                .from(DataSource.database(db))
                .where(Expression.property("type").equalTo("task-list.user")
                        .and(Expression.property("taskList.id").equalTo(listID)))
                .toLive();
    }
}