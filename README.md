# Hardware Server

This Micro Service allows a physical **Line Display** (also called a **Customer Display**) to be **accessed as a web service**. It's written in **.Net Core 3.1** and is part of a concept which would allow devices to be connected to web based applications running in a local browser on **Windows**, **Linux** or **OSX**. I sucssesfully run this on a Windows 10 PC and a Rasberry Pi running Raspbian. If the concept is extended further devices could include *POS Printers*, *Cash Drawers*, *Line Displays*, *Barcode Scanners*, *Magnet Stripe Readers* & *NFC Readers*.

The concept has been tested against an **Epson DM-D102**. It's part of a 20 year old **TM-H5000II** which includes a **TM-T88II** thermal receipt printer and an impact slip printer. All work perfectly - which shows just how rugged they are. This will work with any modern Epson line display which supports ESC/POS commands.

Line Displays are generally 2 rows of 20 columns. In 35 years of developing Point Of Sale systems I have only every come across a couple which weren't. Clear information is a challenge with such limited space.

This display will wrap onto the other line when characters are written to the last column. So writing 40 characters will always leave the row & column back where they started.

At time of intial repository creation there was a [running demo](https://www.wealthall.com/hardware) which was connected to a line display on my desk.
