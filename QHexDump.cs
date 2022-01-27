// QHexDump v1.0 (c) 2022 Sensei (aka 'Q')
// Dumps the specified file in hexadecimal and ASCII format. Supports interactive mode in the console.
//
// Usage:
// QHexDump [-h|--help|/?] [-v|--verbose] [-i|--interactive] [-n|--no-guides] [-x|--hex-only] [-o|--offset] [-l|--length] [-c|--columns] [-r|--rows] [-g|--group] filename
//
// Compilation:
// %SYSTEMROOT%\Microsoft.NET\Framework\v3.5\csc QHexDump.cs

using System;
using System.IO;

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
      public Options() {
         filename = null;
         verbose = false;
         interactive = false;
         guides = true;
         ascii = true;
         offset = 0;
         length = 0;
         columns = 32;
         rows = 32;
         group = 4;
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
                  Console.Error.WriteLine( "Illegal offset!" );
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-l" ) || arg.Equals( "--length" ) ) {
               i++;
               try {
                  length = Int32.Parse( args[ i ] );
                  if( length <= 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( "Illegal length!" );
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-c" ) || arg.Equals( "--columns" ) ) {
               i++;
               try {
                  columns = Int32.Parse( args[ i ] );
                  if( columns <= 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( "Illegal columns!" );
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-r" ) || arg.Equals( "--rows" ) ) {
               i++;
               try {
                  rows = Int32.Parse( args[ i ] );
                  if( rows <= 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( "Illegal rows!" );
                  Console.Error.WriteLine( e.Message );
                  Environment.Exit( 20 );
               }
            } else if( arg.Equals( "-g" ) || arg.Equals( "--group" ) ) {
               i++;
               try {
                  group = Int32.Parse( args[ i ] );
                  if( group <= 0 ) throw( new FormatException() );
               } catch( Exception e ) {
                  Console.Error.WriteLine( "Illegal group!" );
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
      }
   }

   public static string READABLE = "!@#$%^&*()_+|}{\\\":?><,./;'[]=-";
   private static bool IsCharReadable( char chr ) {
      if( Char.IsLetter( chr ) ) return( true );
      if( Char.IsDigit( chr ) ) return( true );
      if( READABLE.IndexOf( chr ) != -1 ) return( true );
      return( false );
   }

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
            FileStream stream = File.Open( filename, FileMode.Open, FileAccess.Read, FileShare.Read );
            byte [] buffer = new byte[ options.length ];
            stream.Seek( options.offset, SeekOrigin.Begin );
            stream.Read( buffer, 0, options.length );
            stream.Close();
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
            group = 0;
            for( int i = 0; i < options.length; i++ ) {
               if( options.guides ) {
                  if( column == 0 ) {
                     Console.Write( String.Format( "{0}", row + 1 ).PadLeft( 3 ).PadRight( 4 ) );
                     Console.Write( String.Format( "{0:X} ", options.offset + i ).PadLeft( 8 + 1 ) );
                  }
               }
               Console.Write( "{0:X2}", buffer[ i ] );
               column++;
               if( column >= options.columns ) {
                  if( options.ascii ) {
                  Console.Write( " | " );
                  for( int j = start; j <= i; j++ ) {
                     char chr = (char) buffer[ j ];
                     if( !IsCharReadable( chr ) ) {
                        chr = UNRECOGNIZED;
                     }
                     Console.Write( chr );
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
      ConsoleKey key;
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

         ConsoleKeyInfo input = Console.ReadKey( true );
         key = input.Key;
         if( key == ConsoleKey.UpArrow ) {
            options.offset = Math.Max( 0, options.offset - options.columns );
         } else if( key == ConsoleKey.DownArrow ) {
            options.offset = Math.Max( 0, options.offset + options.columns );
         } else if( key == ConsoleKey.LeftArrow ) {
            options.group = Math.Max( 1, options.group - 1 );
         } else if( key == ConsoleKey.RightArrow ) {
            options.group = Math.Min( options.columns, options.group + 1 );
         } else if( key == ConsoleKey.PageUp ) {
            options.offset = Math.Max( 0, options.offset - options.length );
         } else if( key == ConsoleKey.PageDown ) {
            options.offset = Math.Max( 0, options.offset + options.length );
         } else if( key == ConsoleKey.Home ) {
            options.offset = 0;
         } else if( key == ConsoleKey.End ) {
            options.offset = Math.Max( 0, length - options.length );
         } else if( key == ConsoleKey.G ) {
            options.guides = !options.guides;
         } else if( key == ConsoleKey.A ) {
            options.ascii = !options.ascii;
         } else if( key == ConsoleKey.V ) {
            options.verbose = !options.verbose;
         }
      } while( key != ConsoleKey.Escape );
   }

   public static void Help() {
      Console.WriteLine( "QHexDump v1.0 (c) 2022 Sensei (aka 'Q')" );
      Console.WriteLine( "Dumps the specified file in hexadecimal and ASCII format. Supports interactive mode in the console." );
      Console.WriteLine();
      Console.WriteLine( "Usage:" );
      Console.WriteLine( "QHexDump [-h|--help|/?] [-v|--verbose] [-i|--interactive] [-o|--offset] [-l|--length] [-c|--columns] [-r|--rows] [-g|--group] filename" );
      Console.WriteLine();
      Console.WriteLine( "Examples:" );
      Console.WriteLine( "QHexDump data.exe" );
      Console.WriteLine( "QHexDump --offset 10000 --length 10000 --columns 100 --group 16 data.exe" );
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
