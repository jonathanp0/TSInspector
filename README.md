# TSInspector
A tool for experimenting with the Rail Simulator External Interface API. It communicates with Train Simulator via a TSConductor server so that inspection of the simulation state can be easily done on a seperate computer whilst the simulator is in operation.

[![TS Inspector.png](https://s30.postimg.org/tyvpo8329/TS_Inspector.png)](https://postimg.org/image/db47lq8al/)

## User Guide
TS Inspector is very simple to use. After connecting to a TSConductor server using the Server->Connect menu item, the properties of all available variables are displayed, and updated every second. If any problems occur, they will be displayed in a message box or in the status bar. To set the value of a variable, click it's row, then enter the new value and click 'Send'. Note that some variables are read only and setting them will have no effect on the simulation.

##Requirements
TS Inspector requires .NET Version 4.5 or later. If you have Windows 8 or newer than you should have this already installed, otherwise you might need to obtain it from the Microsoft website.

TS Inspector needs a TSConductor server running on the computer which Train Simulator is running on. You can download it here - http://tsconductor.trainsim.org/.

## Thanks

TS Inpector would not be possible without the work of Dovetail Games Technical Designer Matt Peddlesden, who created the enhanced External Interface API that allows access to all internal simulation variables, and Daniel Jackob who created TSConductor.

## License
Licensed under the MIT License

Copyright (c) 2017 Jonathan Pilborough

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
