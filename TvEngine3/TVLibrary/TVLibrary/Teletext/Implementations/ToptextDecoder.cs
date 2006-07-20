/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Teletext
{
  public class ToptextDecoder
  {
    #region constants
    const int NotPresent = 0x0;
    const int SubtitlePage = 0x01;
    const int ProgramInfoBlockPageSingle = 0x02;
    const int ProgramInfoBlockPageMulti = 0x03;
    const int BlockPageSingle = 0x04;
    const int BlockPageMulti = 0x05;
    const int GroupPageSingle = 0x06;
    const int GroupPageMulti = 0x07;
    const int NormalPage = 0x08;
    const int NormalPageInfo = 0x09;
    const int NormalPageMulti = 0x0a;
    const int NormalPageMultiInfo = 0x0b;
    const int DontCare = 0x0e;
    const int EndTable = 0x0f;

    const int TOP_BASIC_PAGE = 0x1f0;
    const int TOP_MULTI_PAGE = 0x1f1;
    const int TOP_ADDITIONAL_PAGE = 0x1f2;
    #endregion

    #region variables
    int[] _pageType = new int[900];
    int[] _pageSubCount = new int[900];
    string[] _pageDescription = new string[900];
    int _pageRed = -1;
    int _pageGreen = -1;
    int _pageYellow = -1;
    int _pageBlue = -1;
    byte[] _row24 = new byte[42];
    #endregion

    public int Red
    {
      get
      {
        return _pageRed;
      }
    }
    public int Green
    {
      get
      {
        return _pageGreen;
      }
    }
    public int Yellow
    {
      get
      {
        return _pageYellow;
      }
    }
    public int Blue
    {
      get
      {
        return _pageBlue;
      }
    }

    public byte[] Row24
    {
      get
      {
        return (byte[])_row24.Clone();
      }
    }
    int ConvertToHex(int number)
    {
      if (number < 0) return -1;
      int mag = number / 100; number -= (mag * 100);
      int tens = (number / 10);
      int units = (number % 10);
      return mag * 0x100 + tens * 0x10 + units;
    }
    public bool Decode(TeletextPageCache cache, int pageNumber)
    {
      int red, green, yellow, blue;
      string nextGroup, nextBlock;
      if (!GetPageLinks(cache, pageNumber, out red, out green, out yellow, out blue, out nextGroup, out nextBlock)) return false;
      _pageRed = ConvertToHex(red);
      _pageGreen = ConvertToHex(green);
      _pageYellow = ConvertToHex(yellow);
      _pageBlue = ConvertToHex(blue);
      if (nextGroup == String.Empty) nextGroup = yellow.ToString();
      if (nextBlock == String.Empty) nextBlock = green.ToString();

      Hamming.SetPacketNumber(0, ref _row24, pageNumber, 24);
      int spaces = 40 - (nextGroup.Length + nextBlock.Length + 3 + 3 + 4);
      spaces /= 3;
      string line = ((char)TeletextPageRenderer.Attributes.AlphaRed) + red.ToString();
      for (int x = 0; x < spaces; x++) line += " ";

      line += ((char)TeletextPageRenderer.Attributes.AlphaGreen) + nextGroup;
      for (int x = 0; x < spaces; x++) line += " ";

      line += ((char)TeletextPageRenderer.Attributes.AlphaYellow) + nextBlock;
      for (int x = 0; x < spaces; x++) line += " ";
      line += ((char)TeletextPageRenderer.Attributes.AlphaCyan) + blue.ToString();
      for (int i = 0; i < line.Length && i <= 40; ++i)
      {
        _row24[2 + i] = (byte)line[i];
      }
      return true;
    }
    public bool GetPageLinks(TeletextPageCache cache, int pageNumber, out int redPage, out int greenPage, out int yellowPage, out int bluePage, out string nextGroup, out string nextBlock)
    {
      //red   = previous page in current
      //green = first page of next group
      //yellow= first page of next block
      //blue  = next page
      int mag = pageNumber / 0x100;
      int tens = (pageNumber - mag * 0x100) / 0x10;
      int units = (pageNumber - mag * 0x100) - tens * 0x10;
      int decimalPage = mag * 100 + tens * 10 + units;

      Clear();
      nextGroup = "";
      nextBlock = "";
      redPage = greenPage = yellowPage = bluePage = -1;

      if (!DecodeBasicPage(cache)) return false;
      DecodeMultiPage(cache);
      DecodeAdditionalPages(cache);

      //red: prev page
      for (int page = decimalPage - 1; page > 100; page--)
      {
        if (_pageType[page] != NotPresent)
        {
          redPage = page;
          break;
        }
      }
      if (redPage == -1)
      {
        for (int page = 899; page > 100; page--)
        {
          if (_pageType[page] != NotPresent)
          {
            redPage = page;
            break;
          }
        }
      }

      for (int page = decimalPage + 1; page <= 899; page++)
      {
        if (yellowPage == -1)
        {
          if (_pageType[page] == GroupPageSingle || _pageType[page] == GroupPageMulti)
          {
            yellowPage = page;
            nextGroup = _pageDescription[yellowPage];
            if (nextGroup == null) nextGroup = yellowPage.ToString();
          }
        }
        if (greenPage == -1)
        {
          if (_pageType[page] == BlockPageSingle || _pageType[page] == BlockPageMulti)
          {
            greenPage = page;
            nextBlock = _pageDescription[greenPage];
            if (nextBlock == null) nextBlock = greenPage.ToString();
          }
        }
      }

      for (int page = 100; page <= decimalPage; page++)
      {
        if (yellowPage == -1)
        {
          if (_pageType[page] == GroupPageSingle || _pageType[page] == GroupPageMulti)
          {
            yellowPage = page;
            nextGroup = _pageDescription[yellowPage];
            if (nextGroup == null) nextGroup = yellowPage.ToString();
          }
        }
        if (greenPage == -1)
        {
          if (_pageType[page] == BlockPageSingle || _pageType[page] == BlockPageMulti)
          {
            greenPage = page;
            nextBlock = _pageDescription[greenPage];
            if (nextBlock == null) nextBlock = greenPage.ToString();
          }
        }
      }


      //blue : next page
      for (int page = decimalPage + 1; page <= 899; page++)
      {
        if (_pageType[page] != NotPresent)
        {
          bluePage = page;
          break;
        }
      }
      if (bluePage == -1)
      {
        for (int page = 100; page <= 899; page++)
        {
          if (_pageType[page] != NotPresent)
          {
            bluePage = page;
            break;
          }
        }
      }

      nextGroup = nextGroup.Trim();
      nextBlock = nextBlock.Trim();
      return true;
    }

    public void Clear()
    {
      for (int x = 0; x < _row24.Length; x++)
        _row24[x] = 32;
      _pageRed = -1;
      _pageGreen = -1;
      _pageYellow = -1;
      _pageBlue = -1;
      _pageType = new int[900];
      _pageSubCount = new int[900];
      _pageDescription = new string[900];
    }

    static public bool IsTopTextPage(int pageNumber, int subPageNumber)
    {
      if (pageNumber >= TOP_BASIC_PAGE && pageNumber < 0x1fd) return true;
      return false;
    }

    bool DecodeBasicPage(TeletextPageCache cache)
    {
      //basic toppage contains information which teletext pages are onair
      //and if onair if it has subpages or not
      //also contains the type for each page
      if (!cache.PageExists(TOP_BASIC_PAGE)) return false;
      byte[] basicPage = cache.GetPage(TOP_BASIC_PAGE, 0);
      for (int pageNr = 100; pageNr <= 899; pageNr++)
      {
        int row = ((pageNr - 100) / 40) + 1;
        int col = ((pageNr - 100) % 40) + 2;
        byte data = Hamming.Decode[basicPage[row * 42 + col]];
        if (data == ProgramInfoBlockPageMulti ||
            data == BlockPageMulti ||
            data == GroupPageMulti ||
            data == NormalPageMulti ||
            data == NormalPageMultiInfo
            )
        {
          //page is onair and has subpages
          _pageType[pageNr] = data;
        }
        if (data == ProgramInfoBlockPageSingle ||
            data == BlockPageSingle ||
            data == GroupPageSingle ||
            data == NormalPage ||
            data == NormalPageInfo)
        {
          //page is onair and has subpages
          _pageType[pageNr] = data;
        }
        if (data == NotPresent)
        {
          //page is not onair
          _pageType[pageNr] = data;
          _pageSubCount[pageNr] = 0;
        }
      }
      return true;
    }

    void DecodeMultiPage(TeletextPageCache cache)
    {
      // multi page contains the number of subpages transmitted for each page (100-899);
      if (!cache.PageExists(TOP_MULTI_PAGE)) return;
      byte[] multiPage = cache.GetPage(TOP_MULTI_PAGE, 0);
      for (int pageNr = 100; pageNr <= 899; pageNr++)
      {
        int row = ((pageNr - 100) / 40) + 1;
        int col = ((pageNr - 100) % 40) + 2;
        byte data = Hamming.Decode[multiPage[row * 42 + col]];
        if (data == NotPresent)
        {
          //page is offair
          _pageSubCount[pageNr] = 0;
        }
        else if (data == 0x0a)
        {
          //page has > 10 subpages
          _pageSubCount[pageNr] = 10; //( or more)
        }
        else
        {
          int numberOfSubPages = data - 1;
          _pageSubCount[pageNr] = numberOfSubPages;
        }
      }
    }

    void DecodeAdditionalPages(TeletextPageCache cache)
    {
      //addditional pages carry information to display on line 24
      //
      for (int pageNr = TOP_ADDITIONAL_PAGE; pageNr < 0x1fd; ++pageNr)
      {
        if (!cache.PageExists(pageNr)) continue;
        byte[] additionalPage = cache.GetPage(pageNr, 0);
        int lineCounter = 0;
        while (true)
        {
          int row = 1 + (lineCounter / 2);
          int col = (20 * (lineCounter % 2));
          byte magazine = Hamming.Decode[additionalPage[row * 42 + 2 + col]];
          byte pageTens = Hamming.Decode[additionalPage[row * 42 + 3 + col]];
          byte pageUnits = Hamming.Decode[additionalPage[row * 42 + 4 + col]];
          if (magazine >= 0 && magazine <= 7)
          {
            if (pageTens >= 0x0 && pageTens <= 9)
            {
              if (pageUnits >= 0x0 && pageUnits <= 9)
              {
                string description = String.Empty;
                for (int i = 1; i < 12; ++i)
                {
                  row = 1 + (lineCounter / 2);
                  col = 9 + i + (20 * (lineCounter % 2));

                  description += (char)(additionalPage[row * 42 + col] & 0x7f);
                }
                if (magazine == 0) magazine = 8;
                int pageNo = magazine * 100 + pageTens * 10 + pageUnits;
                if (pageNo >= 100 && pageNo <= 899)
                {
                  _pageDescription[pageNo] = description;
                }
              }
            }
          }
          lineCounter++;
          if (lineCounter >= 43) break;
        }
      }
    }
  }
}
