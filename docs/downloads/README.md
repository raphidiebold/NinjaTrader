# Downloads Folder

## How to add the compiled DLL:

1. **Compile your indicator** in NinjaTrader:
   - Open NinjaScript Editor (F11)
   - Open your VolumeBubbleIndicator.cs
   - Press F5 to compile
   - The compiled DLL will be created automatically

2. **Find the compiled DLL:**
   - Navigate to: `C:\Users\YourUsername\Documents\NinjaTrader 8\bin\Custom\`
   - Look for `VolumeBubbleIndicator.dll`

3. **Create a ZIP file:**
   - Copy the `VolumeBubbleIndicator.dll` file
   - Create a new ZIP file named `VolumeBubbleIndicator.zip`
   - Add the DLL file to the ZIP

4. **Upload to this folder:**
   - Place `VolumeBubbleIndicator.zip` in this `/docs/downloads/` folder
   - Commit and push to GitHub
   - The file will be accessible at: `https://rdindicators.netlify.app/downloads/VolumeBubbleIndicator.zip`

## Alternative: Use GitHub Releases

You can also host the DLL on GitHub Releases:
1. Go to your GitHub repository
2. Click "Releases" → "Create a new release"
3. Upload the ZIP file
4. Use the release download URL in the email

Then update the environment variable `DOWNLOAD_URL` in Netlify with the correct URL.
