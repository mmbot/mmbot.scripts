mmbot.scripts
=============

This is the script catalog for [mmbot](https://github.com/mmbot/mmbot) the C# port of of GitHub's Hubot automation bot. 

[Browse the catalog](http://mmbot.github.io/mmbot.scripts/catalog.html)

## Contributing
To contribute a new script or fix an existing one:
* Raise an issue and tag it with either "new script" or "bug"
* Fork the repo
* Submit a Pull Request

## Updating the Catalog
To update the catalog after adding or modifying a script:
* Pull a local copy of the gh-pages branch
`git pull origin gh-pages`
* Checkout the gh-pages branch locally
`git checkout gh-pages`
* Remove the existing script folder
`rm scripts`
* Checkout the master scripts folder
`git checkout master scripts`
* Run the Generate-Catalog.ps1 powershell script
* Verify the changes in catalog.md and catalog.json
* Verify that the catalog correctly displays using [jekyll](https://help.github.com/articles/using-jekyll-with-pages).  The mmbot.scripts directory will not be available so navigate to http://localhost:4000/catalog.html when testing locally.
* Send a pull request to the gh-pages branch