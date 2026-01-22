using static Facepunch.Constants;

namespace Facepunch.Steps;

internal class SignBinaries() : Step( "SignBinaries" )
{
	protected override ExitCode RunInternal()
	{
		string rootDir = Directory.GetCurrentDirectory();
		string exePath = Path.Combine( rootDir, "game", "bin", "win64" );
		string signtoolPath = Path.Combine( rootDir, "src", "devtools", "bin", "signtool.exe" );

		if ( !File.Exists( signtoolPath ) )
		{
			Log.Error( $"Signtool not found at {signtoolPath}" );
			return ExitCode.Failure;
		}

		if ( !Directory.Exists( exePath ) )
		{
			Log.Error( $"Binary directory not found at {exePath}" );
			return ExitCode.Failure;
		}

		var filesToSign = Directory.EnumerateFiles( exePath, "*.*", SearchOption.TopDirectoryOnly )
			.Where( f => f.EndsWith( ".exe", StringComparison.OrdinalIgnoreCase ) ||
						 f.EndsWith( ".dll", StringComparison.OrdinalIgnoreCase ) )
			.ToList();

		foreach ( var file in filesToSign )
		{
			Log.Info( $"Signing {Path.GetFileName( file )}" );

			bool success = Utility.RunProcess(
				signtoolPath,
				$"sign /n \"Facepunch Studios Ltd\" /q /tr http://timestamp.digicert.com /td SHA256 /fd SHA256 /sm \"{file}\"",
				exePath
			);

			if ( !success )
			{
				Log.Error( $"Failed to sign {file}" );
				return ExitCode.Failure;
			}
		}

		Log.Info( $"Successfully signed {filesToSign.Count} files." );
		return ExitCode.Success;
	}
}
