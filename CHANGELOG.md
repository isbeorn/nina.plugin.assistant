# Target Scheduler

## 4.3.5.0 - 2024-03-08
* Fixed problem with CSV import due to NINA package updates

## 4.3.4.0 - 2024-02-23
*## 4.3.5.0 - 2024-03-08
* Fixed problem with CSV import due to NINA package updates

 Added toggle in Projects navigation to color projects/targets by whether they are active or not
* Added toggle in Projects navigation to show/hide projects/targets by whether they are active or not
* Added copy/paste/reset for Project Scoring Rule Weights

## 4.3.3.0 - 2024-02-15
* Refactored target and exposure planning percent complete handling

## 4.3.2.1 - 2024-02-12
* Fixed exposure completion reversion caused by previous percent complete rule fix

## 4.3.2.0 - 2024-02-06
* Fixed bug in percent complete scoring rule for completed exposure plans

## 4.3.1.0 - 2024-02-02
* Another tweak to TS Condition to ensure loop remains completed
* Fixed bug where target from Framing Wizard would appear to replace target in TS target management panel
* Code clean up

## 4.3.0.0 - 2024-01-26
* Fixed issue where TS Condition wasn't working when called in outer containers
* Increased timeout for sync client registration
* Added validation of TS Container triggers and custom event containers
* Stopped cloning of TS Container triggers into plan sub-container (now run normally)
* Added additional logging of sequence item lifecycle events

## 4.2.0.0 - 2023-12-28
* Added ability to bulk load targets from CSV files

## 4.1.2.2 - 2023-12-21
* Fixed bug in readout mode handling
* Fixed bug with Percent Complete and Mosaic Complete scoring rules if image grading is off

## 4.1.2.0 - 2023-12-18
* Fixed bug in smart plan window - was skipping projects incorrectly
* Fixed another bug with determining target completed
* You can now choose to delete acquired image records when deleting the associated target
* If running as a sync client, TS Condition will now use the server's data for the targets remain or projects remain checks

## 4.1.1.3 - 2023-12-15
* Fixed bug in TS Flats with project flats cadence > 1
* Fixed bug with determining target completeness with exposure throttling
* Fixed missing TS version in TS log

## 4.1.1.1 - 2023-12-14
* Fixed bug in TS Condition - check wasn't running the first time through
* Immediate flats wasn't handling Repeat Flat Set off correctly
* Immediate flats instruction will now open a flip-flat cover when done
* Updated for latest NINA 3 beta libraries

## 4.1.0.8 - 2023-12-12
* Added support for taking automated flats
* Optimized the condition check in Target Scheduler condition
* Target Scheduler Container instruction has a new custom event container: _After Each Target_
* Added a 'need flats' check to Target Scheduler condition

## 4.0.5.1 - 2023-11-26
* Improved handling when TS is canceled/interrupted which means it behaves better in safety scenarios and with Powerups safety controls.

## 4.0.5.0 - 2023-11-17
* Added image grading on FWHM and Eccentricity (requires Hocus Focus plugin)
* Added option to move rejected images to a 'rejected' directory
* Added ability to purge acquired image records by date or date/target
* Added CSV output for acquired image records
* Added better support for the Center After Drift trigger (see release notes)
* Added smarter determination of plan stop times
* Added ability to delete all target exposure plans
* The rule weight list is now sorted when displayed
* Added target rotation and ROI to the set of data saved for acquired images.  A future release will use these values when selecting 'like' images for grading.
* Fixed issue where target rotation wasn't being sent to Framing Wizard
* Added experimental support for synchronization across multiple instances of NINA
* All sequencer instructions moved to new category "Target Scheduler"

## 3.3.3.1 - 2023-10-11
* Fixed bug with exposure planner.

## 3.3.3.0 - 2023-09-19
* Fixed edge case bug with custom horizons.

## 3.3.2.0 - 2023-09-07
* Fixed problem with override exposure ordering. Unfortunately, any existing override order had to be cleared (automatically) for this fix.  You'll have to manually redo any that you had already created.

## 3.3.1.0 - 2023-08-22
* Added ability to override exposure ordering.
* Added Mosaic Completion scoring rule
* Fixed bug with rotation not being set when importing from a saved Sequence Target.
* Fixed bug related to non-existent custom horizon

## 3.2.1.0 - 2023-08-09
* Fixed bug preventing target ROI from being applied properly.

## 3.2.0.0 - 2023-08-07
* Changed the behavior of project minimum altitude: now can be used with or without a custom horizon.  If used with, then the horizon at each azimuth is the greater of (custom horizon + horizon offset) or project minimum altitude.
* Added ability to copy/paste exposure plans.
* Added fixed date range options to Acquired Images viewer and improved performance.
* Added ability to select images in the Acquired Images table by filter used.
* Fixed issue with scheduler preview: wasn't picking up dynamic changes to target database.
* Added 5/10/20 minute options to project minimum time.
* Will automatically unpark the scope if parked before a target slew.
* Fixed the annoying bug related to editing Exposure Templates on Target Exposure Plans.
* Images in the acquired images table will now show 'not graded' as the Reject Reason if grading was disabled when the image completed.
* Now skips useless Target Scheduler Condition checks.

## 3.1.2.0 - 2023-07-20
* The execution of the Before/After Target containers was changed to mean run only for new or changed targets. 
* Added a 'View Details' button to the Scheduler Preview to show details of the planning process.  The same information is available in the TS log for actual runs via the sequencer.

## 3.1.0.0 - 2023-07-13
Limited release only.
* Added support for inserting arbitrary instructions to run at various points during scheduler operation: before/after each wait period and before/after each target plan.
* Removed Conditions and Instructions drop areas in the Target Scheduler Container (not used and just confusing).
* Improved the display of running instructions in Target Scheduler Container

## 3.0.0.0 - 2023-07-XX
Limited release only.
* Ported to NINA 3
* Target rotation values will be auto-converted to NINA 3 counter-clockwise notation

## 0.8.0.0 - 2023-06-12
* Revised dithering approach (see release notes)
* Now does a center with rotation even if target rotation is zero

## 0.7.1.1 - 2023-05-30
* Fixed problem with missing parent for internal container

## 0.7.1.0 - 2023-05-25
* Added support for meridian window restriction
* Added default exposure time to Exposure Templates
* Added airmass to acquired image data detail display
* Added option to park the mount while waiting for next target
* Added option to throttle exposure planning when not grading
* Added option to accept all improvements in image grading
* Added loop condition to support outer loops for safety or multi-night
* Fixed problem with ROI exposure capture
* Fixed problem with including rejected exposure plans

## 0.6.0.0 - 2023-04-27
* Added validation to detect when Loop Conditions or Instructions are added to the TS container

## 0.5.0.0 - 2023-04-26
* Added support for importing mosaic panels from Framing Assistant

## 0.4.1.0 - 2023-04-25
* Added support for managing profile preferences
* Added image grader reject reason to acquired image data

## 0.4.0.0 - 2023-04-24
* First cut at image grader

## 0.3.0.0 - 2023-04-21
* Removed start and end date fields from projects
* Created a custom log for the plugin
* Added support for database migration scripts
* Fixed bug with plan end time

## 0.2.0.1 - 2023-04-20
* Increased the timeout in the image save watcher for DB updates
* Fixed problem saving a sequence as a sequence template

## 0.2.0.0 - 2023-04-02
* Major refactoring of the plugin sequence containers.
* Added Setting Soonest scoring rule.  Although the database schema hasn't changed, any projects created prior to this release will not be able to use this rule.

