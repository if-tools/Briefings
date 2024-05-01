# Briefings
A website that allows you to create flight briefings for quick and organized flight planning.

## Features
- Create flight briefings that contain the minimum info needed for an Infinite Flight flight. Share them with other people for easier group flying, or plan your future flights by keeping a list of your briefings.
- Attach any images to your briefings. These can be aeronautical charts, screenshots or any other pictures needed for your flight.
- Include airport METARs in your briefings. They are fetched each time you load a briefing, so they are always up-to-date.
- Edit your briefings. Add any additional info after creating a briefing in case you've missed it during creation, or edit the flight info to keep the briefing up-to-date.
- Search created briefings. Find your next destination by searching briefings created by other users.
- Make your briefings private and hide them from search, or keep them public for other users to see.
- See your last viewed and created briefings on the main page. 

Please [open an issue](https://github.com/if-tools/Briefings/issues/new?assignees=&labels=enhancement&template=feature_request.md) if you'd like to see more features added!

## Building
IF-Tools Briefings is built with ASP.NET Core 8.0 and Blazor. It uses MongoDB as the database provider and S3 for attachment storage. 
Follow these steps to build and run a local copy of the app:

1. Clone this repository using `git clone`.
2. Go to the project folder (`IF-Tools Briefings`).
3. Create a `.env` file based on the provided `.env.example` according to your MongoDB and S3 configurations.

If you want to use MongoDB Atlas, you only need to specify your connection string in `MONGO_DB_CONN_STRING`, other fields can be left blank.

### Docker
#### Instructions:
1. Build the image and run the app using
    ```bash
    $ docker-compose up --build
    ```
2. IF-Tools Briefings will now be accessible at `localhost:6080`. If `MONGO_DB_CONN_STRING` is not set, it will use a local MongoDB instance provided by the `mongodb` container.

### Native
#### Requirements:
- .NET 8.0

#### Instructions:
1. Build and run the app using
   ```bash
   $ dotnet run
   ```
2. (Optional) Run the local MongoDB environment using
   ```bash
   $ docker-compose up -d mongodb
   ```
   MongoDB will now be running at `localhost:27017`, user: `admin`, password: `admin`, database: `briefings`.

**Note**: For the Get Flight Plan from IF feature to work, you must have [IF-Tools](https://github.com/if-tools/IF-Tools) running on `http://localhost:5001`.

## Contributing
Fork the `develop` branch of this repo to get started, follow the above instructions to build the app, and then open a new Pull Request when you've made your desired changes (don't forget to describe them). Thanks for your help!

## Credits
IF-Tools Briefings uses [FilePond](https://pqina.nl/filepond/) for file uploads and [MagnificPopup](https://dimsemenov.com/plugins/magnific-popup/) for image galleries.  
