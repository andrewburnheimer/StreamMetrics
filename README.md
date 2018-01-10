# StreamMetrics
CLI program to derive SMPTE ST 2110 metrics from input .pcap files


## License

The user of this software is free to use it for any purpose, to distribute it,
to modify it, and to distribute modified versions of the software, under the
terms of the *Apache License 2.0* contained in LICENSE.txt, without concern for
royalties.


## Pre-Requisites

* .NET Framework 4.6.1


## Installation

1. Retrieve the installer, contact the author for details.

2. Install the package by double-clicking the installer.

3. After it has completed, open the `C:\Program Files\NBCUniversal\StreamMetrics\`
folder.

4. Right-click the `StreamMetrics.exe` and select *Send to Desktop (create shortcut)*

![post_install_shortcut](/post_install_shortcut.jpg?raw=true "Create shortcut on Desktop")


## Usage

* Run the application on the sample file by dragging the sample file onto the
shortcut.

![drag_drop_pcap_file](/drag_drop_pcap_file.jpg?raw=true "Drag and Drop .pcap file")

Meta-data about the stream should be configured in a file named
`StreamMetrics.ini` in the same sub-directory as the input sample `.pcap` file.
See `Example-StreamMetrics.ini` for details.

Due to the dependency on PcapDotNet, input `.pcap` files must be typical.
Nanosecond resolution timestamps are not supported.

![wireshark_file_type](/wireshark_file_type.jpg?raw=true "Input .pcap files must be typical")



### Example Results

```
= StreamMetrics 1.1.0 =

Reading: C:\Users\...\Vendor1.pcap

Read 1442065 packets

= Summary Statistics =
Min interval over file (in us)=0.00
1st percentile (find outliers, in us)=7.00
5th percentile (set standard, in us)=7.00
Median (50th percentile, in us)=9.00
Average packet interval (in us)=9.19
Packet interval standard dev. (in us)=0.55
95th percentile (set standard, in us)=10.00
99th percentile (find outliers, in us)=10.00
Max interval over file (in us)=12.00

= Packet Interval Numerical Histogram =
(0;1.2] = 101
(1.2;2.4] = 81
(2.4;3.6] = 328
(3.6;4.8] = 2800
(4.8;6] = 417
(6;7.2] = 18
(7.2;8.4] = 60301
(8.4;9.6] = 832317
(9.6;10.8] = 232
(10.8;12] = 2752

= Packet Interval Pictogram =
Width: 1.2
(0;1.2]           :
(1.2;2.4]         :
(2.4;3.6]         :
(3.6;4.8]         :
(4.8;6]           :
(6;7.2]           :
(7.2;8.4]         :#########
(8.4;9.6]         :##########################################################
(9.6;10.8]        :
(10.8;12]         :

= ST 2110-21 =
Octets to capture the active picture area=5184000
Number of packets per frame of video, N_pkts=3631
Period between consecutive frames of video, T_FRAME (in s)=3.34E-02
Sender Type=2110TPW

= Network Compatibility Model Compliance =
Scaled period between packets draining, T_DRAIN (in s)=8.35E-06
Scaling factor, Beta=1.10
Spec. C_MAX (left part)=16
Spec. C_MAX (right part)=5.04
Spec. C_MAX=16
Obs. C_MAX=3
Stream does comply with the Network Compatibility Model of ST 2110-21

= Virtual Receiver Buffer Model Compliance =
Unscaled period between packets draining, T_DRAIN (in s)=9.19E-06
Spec. VRX_FULL (left part)=720
Spec. VRX_FULL (right part)=362.74
Spec. VRX_FULL=720
Obs. Min VRX_FULL=-1
Obs. Max VRX_FULL=280
Obs. Range VRX_FULL=281
Stream does comply with the Virtual Receive Buffer Model of ST 2110-21

Receiver to start rendering after receiving 281 packets.
```

## Contribute

Please fork the GitHub project (http://github.com/andrewburnheimer/StreamMetrics),
make any changes, commit and push to GitHub, and submit a pull request.


### Develop

Developed using Visual Studio Professional 2015 to date.


## Contact

This project was initiated by Andrew Burnheimer.

* Email:
  * andrew.burnheimer@nbcuni.com
* Twitter:
  * @aburnheimer
* Github:
  * http://github.com/andrewburnheimer
