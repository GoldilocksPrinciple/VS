# VinSeek
A Vindictus Packet Logger/Network Analysis Tool

## IMPORTANT
- [Ekinar](https://github.com/Henrimn/Ekinar) is needed for VinSeek to log packets.

## Requirements
- .NET Framework 4.6.1

## References
1. Custom version of [Be.HexBox](https://sourceforge.net/projects/hexbox/)
	* Added support for WPF.

2. Custom version of [BeeSchema](https://github.com/Michael-Kelley/BeeSchema)
	* Added support for VarInt data type.
	* Added support for parsing array of struct.

3. Custom TreeListView control for WPF

## TO-DO LIST:
- Display debug console under hexbox (mostly for displaying error from parsing template).
- Automatically find corresponding template file (in a directory) based on packet's name and auto parsing.
- Automatically parse all templates on 1 selected packet and calculate match %.
