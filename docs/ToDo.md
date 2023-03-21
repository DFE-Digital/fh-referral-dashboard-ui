# ToDo

* error pages into rcl with auto routing to them

* move rcl into sharedkernel

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