# ToDo

* stories: health check, error handling, move shared razor library into sharedkernel, spike: create npm with styles & scripts, security headers

* error pages into rcl with auto routing to them

* move rcl into sharedkernel

* styles: same as ts below, or create npm package, with dependencies on govuk and moj? see https://github.com/alphagov/govuk-frontend

To create an npm package that contains files for use by the consumer, you can follow these steps:

Create a new directory for your project and navigate to it in your terminal.

Initialize a new npm package by running the following command in your terminal:

csharp
Copy code
npm init
This will guide you through a series of prompts to set up your package's metadata, such as its name, version, description, etc.

Create a new directory in your project directory to hold the files that you want to include in your package. For example, you could create a directory called lib or assets.

Place the files that you want to include in your package in this directory.

In your package's root directory, create a file called .npmignore (note the dot at the beginning of the filename). This file should contain a list of files and directories that you want to exclude from your package. For example, if you have a README.md file in your project directory that you don't want to include in your package, you can add it to your .npmignore file like this:

Copy code
README.md
You can also use wildcards to exclude entire directories or file types. For example, if you want to exclude all .test.js files from your package, you can add this line to your .npmignore file:

Copy code
**/*.test.js
In your package's root directory, create a file called package.json. This file should contain metadata about your package, such as its name, version, dependencies, etc. It should also contain a files array that lists the files and directories that you want to include in your package. For example, if you have a directory called lib that contains the files you want to include, you can add this to your package.json file:

json
Copy code
"files": [
  "lib"
],
Finally, to publish your package to the npm registry, you can run the following command in your terminal:

Copy code
npm publish
This will create a new version of your package and make it available for others to install and use.

That's it! Your package now contains files that can be used by the consumer.

* add ts for GA etc into rcl, then...

Yes, there is a Node.js package called unzipper that can be used to extract files from a NuGet package, so that the extraction can be added to the Gulpfile and done automatically as part of transpiling the TypeScript.

Here's an example of how you can use unzipper to extract TypeScript files from a NuGet package:

Install unzipper by running the following command in the terminal:

```npm install --save-dev unzipper```

In your Gulpfile, import the unzipper module:

```
const unzipper = require('unzipper');
```

Define a Gulp task that extracts the TypeScript files from the NuGet package:

```
gulp.task('extract-ts', function () {
  return gulp.src('path/to/package/package.nupkg')
    .pipe(unzipper.Parse())
    .pipe(gulp.dest('path/to/extracted/files'));
});
```
This task uses gulp.src() to select the NuGet package file, and then uses unzipper.Parse() to extract the TypeScript files from the package. The extracted files are then output to the specified folder using gulp.dest().

Modify your TypeScript compilation task to include the extracted TypeScript files:

```
gulp.task('compile-ts', function () {
  return gulp.src(['path/to/typescript/files/**/*.ts', 'path/to/extracted/files/**/*.ts'])
    .pipe(tsProject())
    .pipe(gulp.dest('path/to/output/folder'));
});
```
This modified task uses gulp.src() to select both the TypeScript files in your project and the extracted TypeScript files. These files are then compiled using gulp-typescript as before.

By adding the extract-ts task to your Gulpfile and modifying your compile-ts task to include the extracted TypeScript files, you can automatically extract the TypeScript files from the NuGet package and compile them as part of your TypeScript build process.