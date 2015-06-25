# Mobile Demos

## Demo 1 - Adding data validation

1. If using quick start
1. Go into insert script and add validation to check length of item.text field
1. Run app and show validation catching
1. Show app still working if validation is fulfilled


## Demo 2 - Adding Push Notifications

1. Configure server side for push (Apple Dev portal / Android portal / Win Portal)
1. Copy certificate / api key / etc to Azure portal
1. Set up push on client
1. Enable push from server to client


## Demo 3 - Adding Auth

1. Lock table operations down to authenticated users
1. Attempt to access and show 401 on client
1. Set up auth provider and mobile service in portal
1. Add auth code to client
1. Show auth working
1. Add logic to insert script to tie data to user
1. Add logic to read script to only fetch data user can access

## Demo 4 - Scaling

1. Go to scaling tab
1. Go to Standard mode
1. Turn on auto-scaling
1. Explain that additional units will only be turned on / used if you need them based off daily API calls

## Demo 5 - Using the CLI

1. Open the CLI tools
1. Perform some commands
1. azure mobile list
1. azure mobile table list <service-name>
