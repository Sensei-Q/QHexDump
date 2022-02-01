// QHexDump v1.1 (c) 2022 Sensei (aka 'Q')
// Dumps the specified file in hexadecimal and ASCII format. Supports interactive mode in the console.
//
// Usage:
// QHexDump [-h|--help|/?] [-v|--verbose] [-i|--interactive] [-n|--no-guides] [-x|--hex-only] [-o|--offset] [-l|--length] [-c|--columns] [-r|--rows] [-g|--group] [-f|--find <ascii>] filename
//
// Compilation:
// %SYSTEMROOT%\Microsoft.NET\Framework\v3.5\csc QHexDump.cs
//
// TODO: enter hex and ASCII to find in interactive mode
//

using System;
using System.IO;
using System.Text;

public class QHexDump {
   public class Options {
      public string filename;
      public bool verbose;
      public bool interactive;
      public bool guides;
      public bool ascii;
      public long offset;
      public int length;
      public int columns;
      public int rows;
      public int group;
      public string[] find;
      public Options() {
         filename = "";
         verbose = false;
         interactive = false;
         guides = true;
         ascii = true;
         offset = 0;
         length = 0;
         columns = 32;
         rows = 32;
         group = 4;
         find = new string[ 0 ];
      }

      public void ParseArgs( string [] args ) {
         for( int i = 0; i < args.Length; i++ ) {
            string arg = args[i];
            if( arg.Equals( "-h" ) || arg.Equals( "--help" ) || arg.Equals( "/?" ) ) {
               Help();
               Environment.Exit( 0 );
            } else if( arg.Equals( "-v" ) || arg.Equals( "--verbose" ) ) {
               verbose = true;
            } else if( arg.Equals( "-i" ) || arg.Equals( "--interactive" ) ) {
               interactive = true;
            } else if( arg.Equals( "-n" ) || arg.Equals( "--no-guides" ) ) {
               guides = false;
            } else if( arg.Equals( "-x" ) || arg.Equals( "--hex-only" ) ) {
               ascii = false;
            } else if( arg.Equals( "-o" ) || arg.Equals( "--offset" ) ) {
               i++;
               try {
                  offset = Int64.Parse( args[ i ] );
                  if( offset < 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-l" ) || arg.Equals( "--length" ) ) {
               i++;
               try {
                  length = Int32.Parse( args[ i ] );
                  if( length <= 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-c" ) || arg.Equals( "--columns" ) ) {
               i++;
               try {
                  columns = Int32.Parse( args[ i ] );
                  if( columns <= 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-r" ) || arg.Equals( "--rows" ) ) {
               i++;
               try {
                  rows = Int32.Parse( args[ i ] );
                  if( rows <= 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-g" ) || arg.Equals( "--group" ) ) {
               i++;
               try {
                  group = Int32.Parse( args[ i ] );
                  if( group <= 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-f" ) || arg.Equals( "--find" ) ) {
               i++;
               try {
                  find = args[ i ].Split( ',' );
               } catch( Exception e ) {
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( i == args.Length - 1 ) {
               filename = arg;
            } else {
               Console.Error.WriteLine( "Unknown argument \"{0}\"!", arg );
               Environment.Exit( 20 );
            }
         }
         if( length == 0 ) {
            length = columns * rows;
         } else if( length > 0 ) {
            rows = ( length + columns - 1 ) / columns;
         }
         if( find.Length >= HIGHLIGHTS.Length ) {
            if( verbose ) {
               Console.Error.WriteLine( "Too many strings to find.." );
            }
         }
      }
   }

   public static string READABLE = "!@#$%^&*()_+|}{\\\":?><,./;'[]=-";
   private static bool IsCharReadable( char chr ) {
      if( Char.IsLetter( chr ) ) return( true );
      if( Char.IsDigit( chr ) ) return( true );
      if( READABLE.IndexOf( chr ) != -1 ) return( true );
      return( false );
   }

   private static ConsoleColor [] HIGHLIGHTS = new ConsoleColor[] {
      ConsoleColor.Black,
      ConsoleColor.Red,
      ConsoleColor.Yellow,
      ConsoleColor.Green,
      ConsoleColor.Cyan,
      ConsoleColor.Blue,
      ConsoleColor.DarkRed,
      ConsoleColor.DarkYellow,
      ConsoleColor.DarkGreen,
      ConsoleColor.DarkCyan,
      ConsoleColor.DarkBlue
   };

   public static char UNRECOGNIZED = ' ';

   public static void HexDump( string filename, Options options ) {
      if( File.Exists( filename ) ) {
         try {
            FileInfo fileinfo = new FileInfo( filename );
            long length = fileinfo.Length;
            if( options.verbose ) {
               Console.WriteLine( "Dumping the file \"{0}\" Length {1} (0x{2:X})", filename, length, length );
               Console.WriteLine();
            }

            int border = 0;
            if( options.find.Length > 0 ) {
               for( int i = 0; i < options.find.Length; i++ ) {
                  string find = options.find[ i ];
                  border = Math.Max( border, find.Length );
               }
            }

            int border_start = border;
            int border_end = border;

            if( options.offset < border_start ) {
               border_start = (int) options.offset;
            }

            FileStream stream = File.Open( filename, FileMode.Open, FileAccess.Read, FileShare.Read );
            int buffer_length = border_start + options.length + border_end;
            byte [] buffer = new byte[ buffer_length ];
            stream.Seek( options.offset - border_start, SeekOrigin.Begin );
            stream.Read( buffer, 0, buffer_length );
            stream.Close();

            int [] highlights = new int[ buffer_length ];

            StringBuilder builder = new StringBuilder( buffer_length * 2 );
            for( int i = 0; i < buffer_length; i++ ) {
               builder.Append( buffer[ i ].ToString( "X2" ) );
            }
            string hex_buffer = builder.ToString();

            builder = new StringBuilder( buffer_length ); // .NET Framework v3.5 does not have Clear()..
            for( int i = 0; i < buffer_length; i++ ) {
               char chr = (char) buffer[ i ];
               builder.Append( IsCharReadable( chr ) ? chr : UNRECOGNIZED );
            }
            string ascii_buffer = builder.ToString();

            int row = 0;
            int column = 0;
            int group = 0;
            int start = 0;
            if( options.guides ) {
               Console.Write( "".PadRight( 4 ) );
               Console.Write( String.Empty.PadLeft( 8 + 1 ) );
               for( int i = 0; i < options.columns; i++ ) {
                  Console.Write( String.Format( "{0:X}", i ).PadLeft( 2 ) );
                  group++;
                  if( group >= options.group ) {
                     Console.Write( ' ' );
                     group = 0;
                  }
               }
               Console.WriteLine();
               Console.WriteLine();
            }
            if( options.find.Length > 0 ) {
               int color_index = 1;
               foreach( string find in options.find ) {
                  {
                     int index = hex_buffer.IndexOf( find );
                     while( index != -1 ) {
                        int start_index = index / 2;
                        int len = find.Length / 2;
                        for( int i = 0; i < len ; i++ ) {
                           highlights[ start_index + i ] = color_index;
                        }
                        index = hex_buffer.IndexOf( find, index + find.Length );
                     }
                  }
                  {
                     int index = ascii_buffer.IndexOf( find );
                     while( index != -1 ) {
                        int start_index = index;
                        int len = find.Length;
                        for( int i = 0; i < len ; i++ ) {
                           highlights[ start_index + i ] = color_index;
                        }
                        index = ascii_buffer.IndexOf( find, index + find.Length );
                     }
                  }
                  color_index++;
               }
            }
            ConsoleColor old_background_color = Console.BackgroundColor;
            ConsoleColor old_foreground_color = Console.ForegroundColor;
            group = 0;
            for( int i = 0; i < options.length; i++ ) {
               if( options.guides ) {
                  if( column == 0 ) {
                     Console.Write( String.Format( "{0}", row + 1 ).PadLeft( 3 ).PadRight( 4 ) );
                     Console.Write( String.Format( "{0:X} ", options.offset + i ).PadLeft( 8 + 1 ) );
                  }
               }
               int highlight_color_index = highlights[ border_start + i ];
               if( highlight_color_index != 0 ) { // It is slow, which is why it is set up only when it is needed..
                  Console.BackgroundColor = HIGHLIGHTS[ Math.Min( highlight_color_index, HIGHLIGHTS.Length - 1 ) ];
                  Console.ForegroundColor = ConsoleColor.Black;
               }
               Console.Write( hex_buffer.Substring( ( border_start + i ) * 2, 2 ) );
               if( highlight_color_index != 0 ) { // It is slow, which is why it is set up only when it is needed..
                  Console.BackgroundColor = old_background_color;
                  Console.ForegroundColor = old_foreground_color;
               }
               column++;
               if( column >= options.columns ) {
                  if( options.ascii ) {
                     Console.Write( " | " );
                     for( int j = start; j <= i; j++ ) {
                        highlight_color_index = highlights[ border_start + j ];
                        if( highlight_color_index != 0 ) { // It is slow, which is why it is set up only when it is needed..
                           Console.BackgroundColor = HIGHLIGHTS[ Math.Min( highlight_color_index, HIGHLIGHTS.Length - 1 ) ];
                           Console.ForegroundColor = ConsoleColor.Black;
                        }
                        Console.Write( ascii_buffer[ border_start + j ] );
                        if( highlight_color_index != 0 ) { // It is slow, which is why it is set up only when it is needed..
                           Console.BackgroundColor = old_background_color;
                           Console.ForegroundColor = old_foreground_color;
                        }
                     }
                     start = i + 1;
                  }
                  Console.WriteLine();
                  column = 0;
                  group = 0;
                  row++;
                  if( row >= options.rows ) {
                     break;
                  }
               } else {
                  group++;
                  if( group >= options.group ) {
                     Console.Write( ' ' );
                     group = 0;
                  }
               }
            }
         } catch( Exception e ) {
            Console.Error.WriteLine( e.Message );
            System.Environment.Exit( 20 );
         }
      }
   }

   public static void HexDumpInteractive( string filename, Options options ) {
      ConsoleKeyInfo input;
      do {
         Console.Clear();

         FileInfo fileinfo = new FileInfo( options.filename );
         long length = fileinfo.Length;

         if( options.verbose ) {
            Console.WriteLine( "QHexDump v1.0 (c) 2022 Sensei (aka 'Q')" );
            Console.WriteLine( "Dumps the specified file in hexadecimal and ASCII format. Supports interactive mode in the console." );
            Console.WriteLine();
            Console.WriteLine( "Press Esc to exit interactive mode." );
            Console.WriteLine( "Press Up, Down, Page Up, Page Down, Home, End keys to navigate." );
            Console.WriteLine( "Press Left, Right keys to decrease/increase group width." );
            Console.WriteLine( "Press G key to toggle guides on/off." );
            Console.WriteLine( "Press A key to toggle ASCII on/off." );
            Console.WriteLine( "Press V key to toggle Verbose on/off." );
            Console.WriteLine();
         }

         HexDump( filename, options );

         input = Console.ReadKey( true );
         switch( input.Key ) {
            case ConsoleKey.UpArrow: {
               options.offset = Math.Max( 0, options.offset - options.columns );
               break;
            }
            case ConsoleKey.DownArrow: {
               options.offset = Math.Max( 0, options.offset + options.columns );
               break;
            }
            case ConsoleKey.LeftArrow: {
               options.group = Math.Max( 1, options.group - 1 );
               break;
            }
            case ConsoleKey.RightArrow: {
               options.group = Math.Min( options.columns, options.group + 1 );
               break;
            }
            case ConsoleKey.PageUp: {
               options.offset = Math.Max( 0, options.offset - options.length );
               break;
            }
            case ConsoleKey.PageDown: {
               options.offset = Math.Max( 0, options.offset + options.length );
               break;
            }
            case ConsoleKey.Home: {
               options.offset = 0;
               break;
            }
            case ConsoleKey.End: {
               options.offset = Math.Max( 0, length - options.length );
               break;
            }
            case ConsoleKey.G: {
               options.guides = !options.guides;
               break;
            }
            case ConsoleKey.A: {
               options.ascii = !options.ascii;
               break;
            }
            case ConsoleKey.V: {
               options.verbose = !options.verbose;
               break;
            }
         }
      } while( input.Key != ConsoleKey.Escape );
   }

   public static void Help() {
      Console.WriteLine( "QHexDump v1.0 (c) 2022 Sensei (aka 'Q')" );
      Console.WriteLine( "Dumps the specified file in hexadecimal and ASCII format. Supports interactive mode in the console." );
      Console.WriteLine();
      Console.WriteLine( "Usage:" );
      Console.WriteLine( "QHexDump [-h|--help|/?] [-v|--verbose] [-i|--interactive] [-n|--no-guides] [-x|--hex-only] [-o|--offset] [-l|--length] [-c|--columns] [-r|--rows] [-g|--group] [-f|--find <ascii>] filename" );
      Console.WriteLine();
      Console.WriteLine( "Examples:" );
      Console.WriteLine( "QHexDump data.exe" );
      Console.WriteLine( "QHexDump --offset 10000 --length 10000 --columns 100 --group 20 data.exe" );
      Console.WriteLine( "QHexDump -i -v -f find,me data.exe" );
   }

   public static void Main( string [] args ) {
      if( args.Length > 0 ) {
         try {
            Options options = new Options();
            options.ParseArgs( args );
            if( options.interactive ) {
               HexDumpInteractive( options.filename, options );
            } else {
               HexDump( options.filename, options );
            }
         } catch( Exception e ) {
            Console.Error.WriteLine( e.Message );
            System.Environment.Exit( 20 );
         }
      } else {
         Help();
      }
   }
}
