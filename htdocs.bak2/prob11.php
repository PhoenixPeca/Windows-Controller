<form method="get">
	enter a string: <input type="text" name="num1"/>
	<input type="submit"/>
</form>
<br><br><br>


<?php
if(isset($_GET['num1'])) {
	$input = intval($_GET['num1']);
	$n = str_split($_GET['num1']);
	$fi = array_reverse($n);
	
	foreach($fi as $item) {
		echo $item;
		
	}
	
}
