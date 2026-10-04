# Converter

Load a ZX Spectrum file, palette or image to inspect its contents and convert it between supported formats. All inspection and conversion is done in your browser; files are not uploaded.

Palettes support GPL, JASC-PAL (`.pal`), Paint.NET (`.txt`) and Next NXP files, with indexed RGBA colour swatches. Images support BMP, Spectrum SCR and Next NXI files, with a pixel-perfect preview and 1×, 2×, 4× or 8× zoom. Previews and palettes appear on the Contents tab; File shows metadata. Indexed images also show their palette. SCR previews use the initial FLASH phase; they are not animated.

Conversion buttons include the destination extension. Conversion does not resize images or generate palettes: an indexed BMP must already have a supported screen size and palette to convert to NXI. For example, a 256×384 image can be previewed but cannot be converted directly to a Next screen.

Native formats requiring explicit mode or layout settings are not available in this interface yet. Image previews use the palette supplied by the file or the format's defined rendering palette, not the current state of a physical machine.

<div id="oakio-converter">Loading…</div>
<script src="../coi-serviceworker.js"></script>
<script type="module" src="../assets/javascripts/converter.js"></script>
