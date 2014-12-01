# SpecLog Jenkins Plugin
*SpecLog test status provider for Gherkin tests run by Jenkins/Hudson*

### About
This plugin is the sister of the [SpecRun test statistics plugin](https://github.com/techtalk/SpecLog-Resources/wiki/Gherkin-stats-plugin) bundled with [SpecLog](http://www.speclog.net/) since [version 1.5](https://github.com/techtalk/SpecLog-Resources/wiki/New-in-version-1.5). While the SpecRun plugin &ndash; true to its name &ndash; connects to the [SpecRun](http://www.specrun.com/) adaptive test run scheduler and statistics store server, this plugin uses a [Jenkins](http://jenkins-ci.org)/[Hudson](http://hudson-ci.org) build server to obtain the test run results to decorate your [Gherkin](https://github.com/cucumber/cucumber/wiki/Gherkin) feature files with.

### Configuration
This client side plugin can be configured per repository described in the [SpecLog wiki](https://github.com/techtalk/SpecLog-Resources/wiki/Gherkin-stats-plugin#), choosing the *Configure* option for the *Jenkins Test Statistics* plugin starting from the main menu *Repository settings* option, and clicking the *Configure* button next to the *Gherkin stats plugins* label.

![](http://github.com/quexy/SpecLog.JenkinsPlugin/wiki/plugin_config.png)

Apart from the URL of the Jenkins server and the name of the project, you can optionally specify a user name with which the plugin will authenticate to the server to obtain the test results.

### Notes
SpecLog stores the collected test run results in the local repository so you can see the statistics without being online. This also means, however, that any peers who wish to see the annotations also have to configure the plugin.

After the initial configuration you may have to wait a short while for the plugin to initialize, and download the test run results.

The plugin, whenever online, will periodically query the build server for new results, so your repository will be up to date as long as the build server is reachable.

You can search for requirements with different cumulative test result in the SpecLog repository with the `test:` keyword.