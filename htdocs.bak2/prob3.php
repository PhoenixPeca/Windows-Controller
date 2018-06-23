<form method="get">
	Input the number: <input type="text" name="v"/>
	<input type="submit"/>
</form>
<br><br><br>

<?php
if(isset($_GET['v'])) {
	$v = intval($_GET['v']);
	$r = 0;
	$i = 0;
	$c = 0;

	for($r=1; $i<$v; $i++) {
		for($c=1; $c<=$v; $c++) {
			echo "#";
		}
		echo "<br>";
	}
}