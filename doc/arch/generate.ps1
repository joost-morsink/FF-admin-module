param(
	$BookFile = "book.json",
	$OutDirectory = $null,
	[switch] $Open
)
$Directory = Get-Location

if($null -eq $OutDirectory)
{
	$OutDirectory = (Join-Path $Directory "dist")
}
if (!([IO.Path]::IsPathRooted($OutDirectory))){
	$OutDirectory = [IO.Path]::GetFullPath((Join-Path (Get-Location) $OutDirectory))
}
push-location $Directory
try {
	$book = Get-Content $BookFile | ConvertFrom-Json

	$items = (Get-ChildItem -filter *.drawio | ForEach-Object { [IO.Path]::GetFileNameWithoutExtension($_.Name) })
	if(!(Test-Path images)){
		new-item images -itemtype directory | out-null
	}
	foreach ($item in $items) {
		$drawio = Get-Item "$item.drawio" -ErrorAction SilentlyContinue

		$svg = Get-Item "images/$item.svg" -ErrorAction SilentlyContinue

		if($null -eq $svg -or $drawio.LastWriteTimeUtc -gt $svg.LastWriteTimeUtc){
			/Applications/draw.io.app/Contents/MacOS/draw.io -b 10 -x -o "images/$item.svg" -t -p 0 "$item.drawio" | out-null
		}
	}
	foreach($svg in $items){
		while(!(Test-Path "images/$svg.svg")){
			sleep 1
		}
	}

	if(!(Test-path $OutDirectory)){
		new-item $OutDirectory -itemtype directory | out-null
	}

	new-item tmp -itemtype directory | out-null
	$mdout = $book.out
	if(Test-Path $mdout){
		Remove-Item $mdout
	}
	Get-Content $book.frontmatter | Add-Content -Path $mdout
	Write-Output "`r`n---`r`n" | Add-Content -Path $mdout

	foreach($md in $book.chapters)
	{
		Get-Content $md | Add-Content -Path $mdout
		Write-Output "`r`n" | Add-Content -Path $mdout
	}
	Copy-Item $mdout $OutDirectory
	$md = Get-Item $mdout

		$html = [IO.Path]::ChangeExtension($md.Name, ".html")
		$pdf =  $book.pdf.out #[IO.Path]::ChangeExtension($md.Name, ".pdf")
		$tmp = "tmp/$($md.Name)"
		pandoc $md.Name --template $md.Name -o $tmp 
		pandoc $tmp -o $html -s -c $book.pdf.css -N
		copy-item $html "$OutDirectory/$html"
		copy-item $book.pdf.css $OutDirectory
		wkhtmltopdf --enable-local-file-access --print-media-type -B 25 -T 25 -L 20 -R 20 --dpi 600 --footer-html $book.pdf.footer --header-html $book.pdf.header --outline-depth 4 cover $book.pdf.cover toc --xsl-style-sheet $book.pdf.toc $html "$OutDirectory/$pdf"
		remove-item $html
		remove-item $mdout

		if($Open){
			& "$OutDirectory/$pdf"
		}
	
	remove-item tmp -recurse -force
} finally {
	Pop-Location
}
