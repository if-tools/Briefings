# Briefings
A website that allows you to create flight briefings for quick and organized flight planning.

## Features
These are the current features:
- Create flight briefings that contain the minimum info needed for an IF flight. Share them with other people for easier group flying, or plan your future flights by keeping a list of your briefings.
- Attach any images to your briefings. These can be aeronautical charts, screenshots or any other pictures needed for your flight.
- Edit your already created briefings. Add any additional info after creating a briefing in case you've missed it during creation, or edit the main info to keep the briefing up-to-date.
- Search created briefings. Find your next destination by searching briefings created by other users.
- Make your briefings private and hide them from search, or keep them public for other users to see.

You can always request new features! To do so, [open a new issue](https://github.com/if-tools/Briefings/issues/new?assignees=&labels=enhancement&template=feature_request.md).

## Building
IF-Tools Briefings is built with ASP.NET Core. Follow these steps to build and run a local copy of this website:

1. Clone this repository using `git clone`.
2. Go to the project folder (`IF-Tools Briefings`).
3. Run `dotnet run`. This will build and run the app.

You must have .NET 5.0 installed. You must also have the following environment variable set in your environment:
- `BRIEFINGS_DATABASE_PATH` - The (absolute) path to a local SQLite database. This is required for the app to work correctly. The database must contain 2 tables called `Briefings` and `Attachments`. See the [`Models`](https://github.com/if-tools/Briefings/tree/develop/IF-Tools%20Briefings/Data/Models) folder for these tables' structure.  
Alternatively, you can use the `example-database.db` database located in the root of this repo.

**Note**: For the Get FPL from IF feature to work, you must have [IF-Tools](https://github.com/if-tools/IF-Tools) running on http://localhost:5000.

## Contributing
Want to contribute? That's great, thanks! Fork the `develop` branch of this repo to get started, and then open a new Pull Request when you've made your desired changes (don't forget to describe them).

## Credits
IF-Tools Briefings uses [FilePond](https://pqina.nl/filepond/) for file uploads and [MagnificPopup](https://dimsemenov.com/plugins/magnific-popup/) for image galleries.  
