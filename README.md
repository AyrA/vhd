# VHD

This is a VHD toolkit that allows you to create and edit VHD files.
This does not use any Windows VHD API calls and should work across different platforms.

## Status

This is still in development.

## TODO

Note: items marked as complete are not necessarily usable as-is.
I will likely write the command line interface towards the end.
As of now you have to write your own.

- [X] Header parser and validator
- [X] Basic support for fixed disks
- [X] Create fixed disks
- [ ] Grow and shrink operation
- [ ] Disk cloning
- [ ] Basic support for dynamic disks
- [ ] Create dynamic disks
- [ ] Analyzer and defragmenter for dynamic disks
- [ ] Conversion between dynamic and fixed disks
- [ ] Basic support for differential disks
- [ ] Create differential disks
- [ ] Comiting of differential images

## Documentation

The `docs` folder contains the Virtual Hard Disk Image Format Specification.
Both documents are identical.
