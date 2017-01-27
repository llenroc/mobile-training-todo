//
//  AppDelegate.m
//  Todo
//
//  Created by Pasin Suriyentrakorn on 1/26/17.
//  Copyright © 2017 Pasin Suriyentrakorn. All rights reserved.
//

#import "AppDelegate.h"

#define kDatabaseName @"todo"
#define kUserName @"todo"       // Hardcode Application username used for constructing some document ids

@interface AppDelegate ()

@end

@implementation AppDelegate


- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    NSError *error;
    _database = [[CBLDatabase alloc] initWithName: kDatabaseName error: &error];
    if (!_database) {
        NSLog(@"Cannot open the database: %@", error);
        return false;
    }
    
    // Create database indexes:
    [_database createIndexOn:@[@"type", @"name"] error:nil];
    
    _username = kUserName;
    return YES;
}


- (void)applicationWillResignActive:(UIApplication *)application { }


- (void)applicationDidEnterBackground:(UIApplication *)application { }


- (void)applicationWillEnterForeground:(UIApplication *)application { }


- (void)applicationDidBecomeActive:(UIApplication *)application { }


- (void)applicationWillTerminate:(UIApplication *)application { }


@end