I) Purpose of Project and Development Philosophy

This project is a demonstration of my software skills with regards to both architecture and development.
The development was done with the same attention to detail that would apply to a production application.
I cannot do it any other way, it is an ingrained set of habits. I utilize SOLID principals to create a structure
that is both logical and simple. I want things to be easy to follow. I also strive for simple yet clean and powerful code.
There should be just enough code and not a smidgen more. I want to achieve the fewest lines possible without, of course,
trashing the code with complexity. The rationale is the fewer the lines of code, the less there is for someone
to understand and the less places for a bug to hide. 

Names are descriptive and reflective of their purpose and follow a strict naming convention. Comments are
added when there is a need for some guidance and always for method headers. When modifying code and finding
existing comments to be insufficient, I will expand them, especially if the code is mine. Correctness and
maintainability are the primary goals. TDD is utilized to provide correctness and ensure it is maintianed
as modifications are made. I use extensive testing during development; there is nothing more frustrating
then finding a bug in finished code that was due to a missunderstanding of the requirements.

I want a junior developer to be able to pick my work, understand it and make modifications with a minimum of effort. 

So, give the project a good review and determine its quality. What you see here is what you will get from me.

II) Project Description

I chose as a project a facility that I would find useful and would also be nontrivial. I wanted to display
not only development skills but also user interface design capability. I am in a bookclub (PaperBackSwap.com).
You post books that you want to trade and when asked for one, you mail the book to the requestor with
you paying postage. For each book you send, you can request one for yourself with the sender paying postage.
Tracking the packages is a lot of trouble, especially if there are multiple books simultaneously coming
and going. So the project requirement is to provide an application to allow for the tracking of a single
package and also view the status of all previusly mailed packages with a single button click.

The project is a WPF desktop application written with Visual Studio 2022, Prism 8.1 with Unity, C# 10, and .NET 6. 
The project was originally developed with .NET Core 3.1 but was changed to .NET for deployment reasons.
There were no code changes necessary. VisualStudio is attached to this GitHub Repository and is maintained.
The WPF portion of the project is pure MVVM; there are no codebehind modules. The application uses the USPS
Rest API to perform the tracking. Results containing the tracking information are returned in XML. The
application parses the XML and displays a summary. History is stored in an XML file in the Documents
directory. Color is used to indicate the status of a package. The user can view the detailed tracking by
clicking on an expander. The startup project is TrackerViews.

The project is still under construction with a plan to add UPS tracking and possibly archival of past
tracking. Code features from later versions of C# are being added as their usefulness is encountered.

III) Coding style

Major coding style elements of note are:
1) The left curly brace style is used.
2) Braces are almost exclusively on a separate line.
3) The keyword **var** is never used; its use is set as an error in my environment.
4) Abstract classes are preferred over Interfaces.
5) Interfaces are never used where a class reference works.
6) Repetitive code is collected into methods.
7) Names are camel case with methods names capitalized, variables lower case, globals names start with an underscore.
8) Blank lines are used regularly for clarity.
9) Statements will span multiple lines whenever it enhances clarity.
10) Local variables are used to provide clarity, e.g. "trackingNumberArray[i]" will be extracted to "valueToSum" for use in a block of code.


IV) Note on Cut and Pasting of Code with Example (and more philosophy)

All code is original with the exception of the validation of the UPS tracking number. I have
displayed the "borrowed" code and what was incorporated into the project. It serves to
demonstrate what I mean by "least lines" and the drive for simple code. It is also an example
of what I do to improve legacy code when doing maintenance. I do not consider a fix to be a
"good fix" unless it leaves the application with less lines of code than when I started. This
is usually not that hard since it seems to be common to value speed of implementation over
quality, which makes maintenance much harder than it should be. Cleaning up code in this
fashion also has the possibility of finding as-yet-unencountered bugs in the code. Once
you clear out the brush, you can see the underlying logic better. (Note: I do not mind
working with legacy code since it usually provides more of an interesting challenge than
green field.)

Here is the borrowed code:

        private static bool _ValidateUPSCheckDigit2(string trackingNumber)
        {
            char[] trackingNumberArray = new char[trackingNumber.Length];
            trackingNumberArray = trackingNumber.ToCharArray();
            int checkDigit = 0;
            int sum = 0;
            for (int i = 2; i < trackingNumber.Length - 1; i++)
            {
                if (char.IsDigit(trackingNumberArray[i]) == false)
                {
                    if (Regex.IsMatch(trackingNumberArray[i].ToString(), "[A-H]"))
                    {
                        trackingNumberArray[i] = (char)(Convert.ToInt32(trackingNumberArray.GetValue(i)) - 15);
                    }
                    else if (Regex.IsMatch(trackingNumberArray[i].ToString(), "[I-R]"))
                    {
                        trackingNumberArray[i] = (char)(Convert.ToInt32(trackingNumberArray.GetValue(i)) - 25);
                    }
                    else if (Regex.IsMatch(trackingNumberArray[i].ToString(), "[S-Z]"))
                    {
                        trackingNumberArray[i] = (char)(Convert.ToInt32(trackingNumberArray.GetValue(i)) - 35);
                    }

                }
                if (i % 2 == 0) // adding all odd positions
                {
                    sum += (Convert.ToInt32(trackingNumberArray.GetValue(i)) - 48);
                }
                else
                {
                    sum += 2 * (Convert.ToInt32(trackingNumberArray.GetValue(i)) - 48);
                }

            }
            checkDigit = (int)(Math.Ceiling(sum / 10.0d) * 10) - sum; // round to the next highest ten (71 becomes 80)
            if ((Convert.ToInt32(trackingNumberArray.GetValue(trackingNumber.Length - 1)) - 48) == checkDigit)
            {
                // check digit passes
                return true;
            }
            else
            {
                // check digit fails
                return false;
            }
        }

    Here is what I used:

        private static bool IsvalidUPSCheckDigit(string trackingNumber)
        {
            if (trackingNumber.Length < 18) // Minimum length of UPS tracking number.
                return false;

            int sum = 0;
            int checkDigit = 0;
            char[] trackingNumberArray = trackingNumber.ToUpper().ToCharArray();
            int lastDigit = trackingNumberArray[trackingNumber.Length - 1] - '0';

            // Loop through the array calculating the checksum starting after the "1Z".
            for (int i = 2; i < trackingNumber.Length - 1; i++)
            {
                int valueToSum = trackingNumberArray[i];
                if (valueToSum >= 'A') /* If letter, convert to digit using UPS formula. */
                {
                    if (valueToSum >= 'S')       // Between 'S' and 'Z'
                        valueToSum -= 35;
                    else if (valueToSum >= 'I')  // Between 'I' and 'R'
                        valueToSum -= 25;
                    else                         // Between 'A' and 'H'
                        valueToSum -= 15;
                }

                valueToSum -= '0'; // Convert to the numeric of digit.
                sum += (i % 2 == 0) ? valueToSum : valueToSum * 2; // Double it if is an odd index.
            }

            // Extract single digit from sum.
            // Round to the next highest ten (122 becomes 130) then subtract the sum (which gives 8).
            checkDigit = (int)(Math.Ceiling(sum / 10.0d) * 10) - sum;

            // If the last digit matches the check digit the number is valid.
            return lastDigit == checkDigit;
        }
