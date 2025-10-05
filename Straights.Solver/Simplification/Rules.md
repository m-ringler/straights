<h1 id="BasicRulesOfStr8ts">Basic Rules of Str8ts</h1>

A Str8ts grid is a square grid of $n \times n$ **fields**. The fields are either black or white, and initially either blank or filled with a single integer number. A numbered white field is a *known* white field, a blank white field is an *unknown* white field. Initially, the unknown white fields can have any value v in the range from $1$ to $n$ (which we write as $[1, n]$). Only the unknown white fields can be edited, all other fields are fixed.

The goal of the game is to solve the unknown white fields; that is, to reduce the possible values of each white field to a single value. A grid always has a solution; usually a unique solution. We will not use the uniqueness of the solution for our simplification rules below.

An uninterrupted vertical or horizontal sequence of white fields forms a vertical or horizontal **Block**.

A complete column or row of fields in the grid forms a vertical or horizontal **Column**.

<h2>Block Rule</h2>

<h3 id="BlockConsecutiveNumbers">Consecutive Numbers</h3>

A solved block of $k$ solved fields consists of exactly $k$ consecutive numbers, i.e. all numbers $v$ with $a \leq v < a + k$ in any order, for some number $a$.

<h2>Column Rule</h2>

<h3 id="ColumnNoDuplicateNumbers">No Duplicate Numbers</h3>

In a grid of size $n$, a solved column (or row) may contain each of the numbers in $[1, n]$ at most once. It may not contain any other numbers.

<h1>Derived Simplification Rules</h1>

The following [block rules](#derived-block-rules) and [column rules](#derived-column-rules) can be used to solve a Str8ts grid; they are derived from the [basic game rules](#BasicRulesOfStr8ts) above.

<h2>Notation</h2>

We omit black blank fields, as they are merely block delimiters.

A (white or known black) *field* is written as a box filled with its remaining possible values:

~~~txt
┌───────┐
│ 1 3 7 │
└───────┘
~~~

A *block* is written as an unbroken sequence of fields:

~~~txt
┌─────┬─────┐
│ 1 7 │ 2 8 │
└─────┴─────┘
~~~

A *column* (or row) is written as a sequence of blocks separated by blanks:

~~~txt
┌─────┬─────┐ ┌───┐ ┌─────┬─────┐
│ 1 7 │ 2 8 │ │ 5 │ │ 2 6 │ 3 7 │
└─────┴─────┘ └───┘ └─────┴─────┘
~~~

<h2 id="derived-block-rules">Block Rules</h2>

<h3 id="BlockMinimumAndMaximum">Minimum and Maximum</h3>

<h4>Applicable if</h4>

In a block of size $k$,

* the minimum of the possible values of a field $F$ is $\check m$,
and another field $G$ in the same block has a value $v$ with $v \leq \check m - k$.
-or-
* the maximum of the possible values of a field $F$ is $\hat m$,
and another field $G$ in the same block has a value $v$ with $v \geq \hat m + k$.

<h4>Simplification</h4>

$v$ can be removed from $G$.

<h4>Example</h4>

~~~txt
┌─────────┬────────┬─────────┐
│ 3 4 5 6 │  5 6 7 │ 6 7 8 9 │
└─────────┴────────┴─────────┘
~~~

becomes

~~~txt
┌───────┬───────┬───────┐
│ 4 5 6 │ 5 6 7 │ 6 7 8 │
└───────┴───────┴───────┘
~~~

<h4>Proof</h4>

Let $M$ be the maximum value in the solved block. Then $M \geq \check m$. There are $k - 1$ other fields in the block.
Then, because of the [consecutive numbers rule](#BlockConsecutiveNumbers), the minimum of the values in the solved block is $M - (k - 1)$.
From $v \leq \check m - k$ and $\check m \leq M$, we have $v \leq M - k$ and hence $v < M - (k - 1)$. Hence $v$ cannot be in the solved block.
The proof for the second part is analogous.

<h3 id="BlockNoNeighbors">No Neighbors</h3>

This is a special case of the [disjunct subsets rule](#BlockDisjunctSubsets).

<h4>Applicable if</h4>

The block has more than one field. A field $F$ has a number $a$ and none of the other fields has one of the values $a + 1$ or $a - 1$.

<h4>Simplification</h4>

The value $a$ can be removed from $F$.

<h4>Example</h4>

~~~txt
┌───────┬─────────┐
│ 1 3 7 │ 2 3 5 8 │
└───────┴─────────┘
~~~

becomes

~~~txt
┌─────┬─────┐
│ 1 7 │ 2 8 │
└─────┴─────┘
~~~

<h4>Proof</h4>

This rule follows directly from the [consecutive numbers rule](#BlockConsecutiveNumbers).

<h3 id="BlockTwoValuesFarApart">Two Values Far Apart</h3>

<h4>Applicable if</h4>

Within a block, there is a field $F$ with exactly two remaining possible values, and the difference between these values is greater than or equal to the number $k$ of fields in the block.

<h4>Simplification</h4>

The two values of $F$ can be removed from all other fields in the block.

<h4>Example</h4>

~~~txt
┌─────┬─────────┬───────┐
│ 1 4 │ 1 2 3 5 │ 3 4 6 │
└─────┴─────────┴───────┘
~~~

becomes

~~~txt
┌─────┬───────┬─────┐
│ 1 4 │ 2 3 5 │ 3 6 │
└─────┴───────┴─────┘
~~~

<h4>Proof</h4>

Let the two values of the field $F$ be $a$ and $b$, with $b \geq a + k$.
Assume that another field $G$ has the value $a$ when solved. Then

* $F$ must have the value $b$ because of the [no duplicates rule](#ColumnNoDuplicateNumbers),
* $F$ cannot have the value $b$ because of the [minimum and maximum rule](#BlockMinimumAndMaximum).

So we have a contradiction and our assumption must be wrong.
The proof for $b$ is analogous.

<h3 id="BlockDisjunctSubsets">Disjunct Subsets</h3>

<h4>Applicable if</h4>

* The remaining possible numbers of all fields in a block of size $k$ form two or more disjunct subsets,
i.e. there are gaps in these numbers.
* One of these subsets $S_1$ has cardinality less than $k$, i.e. consists of fewer than $k$ numbers.

<h4>Simplification</h4>

The numbers in $S_1$ can be removed from all fields in the block.

<h4>Example</h4>

~~~txt
┌─────────┬─────────┬───────┐
│ 4 6 7 9 │ 3 4 7 8 │ 3 6 7 │
└─────────┴─────────┴───────┘
~~~

with disjunct subsets

~~~math
S_1 = [3, 4], 
S_2 = [6, 9]
~~~

becomes

~~~txt
┌───────┬─────┬─────┐
│ 6 7 9 │ 7 8 │ 6 7 │
└───────┴─────┴─────┘
~~~

<h4>Proof</h4>

This rule follows directly from the [consecutive numbers](#BlockConsecutiveNumbers) rule.

<h3 id="BlockNFieldsWithNValuesInCertainRange">N Fields with N Values in Certain Range</h3>

<h4 id="certain-range">Definitions</h4>

For a block $B$ with $k$ fields,

* we define the set of remaining possible values $V$ as the union of the remaining possible values of all fields in the block, and
* we define the set of possible solution ranges $R$ as the set of all integer intervals of length $k$ that are contained in $V$, and
* we define the _certain range_ $\mathcal{R}$ of the block as the set intersection of all those intervals.

<h4>Applicable if</h4>

* In a block, there is a set $C$ of $N$ distinct numbers that occur on exactly $N$ fields.
* All numbers of $C$ must occur in the solved block because they are in the certain range of the block $\mathcal{R}$.

<h4>Simplification</h4>

All other numbers can be removed from the fields with numbers from $C$.

<h4>Example</h4>

~~~txt
┌─────────┬───────────┬───────┬─────────┬─────┬─────┐
│ 3 6 7 9 │ 3 4 5 7 8 │ 6 7 9 │ 3 4 6 7 │ 7 8 │ 7 9 │
└─────────┴───────────┴───────┴─────────┴─────┴─────┘
~~~

with certain range

~~~math
\mathcal{R} = [ 3, 8 ] \cap [ 4, 9 ] = [ 4, 8 ]
~~~

the numbers

~~~math
C = \{ 4, 5 \} \subseteq \mathcal{R}
~~~

occur on exactly two blocks, and so we get

~~~txt
┌─────────┬─────┬───────┬───┬─────┬─────┐
│ 3 6 7 9 │ 4 5 │ 6 7 9 │ 4 │ 7 8 │ 7 9 │
└─────────┴─────┴───────┴───┴─────┴─────┘
~~~

<h4>Proof</h4>

We assume that one of the $N$ fields with values from $C$ has a value $v \notin C$ when solved. Then there remain $N - 1$ possible fields where we can put the $N$ elements of $C$, so one of the elements of $C$ cannot be part of the solution. But we know that all the values in $C$ are part of the solution because they are also in $\mathcal{R}$. So, we have a contradiction and the assumption must be wrong.

<h2 id="derived-column-rules">Column Rules</h2>

<h3 id="ColumnRemoveSolvedNumbers">Remove Solved Numbers</h3>

<h4>Applicable if</h4>

The value $a$ of a solved field $F$ occurs in another (unsolved) field $G$ in the same column.

<h4>Simplification</h4>

The value $a$ can be removed from the unsolved field $G$.

<h4>Example</h4>

~~~txt
┌───────┬─────┐ ┌───┐ ┌─────┬─────┐
│ 1 5 7 │ 2 8 │ │ 5 │ │ 2 6 │ 3 7 │
└───────┴─────┘ └───┘ └─────┴─────┘
~~~

becomes

~~~txt
┌─────┬─────┐ ┌───┐ ┌─────┬─────┐
│ 1 7 │ 2 8 │ │ 5 │ │ 2 6 │ 3 7 │
└─────┴─────┘ └───┘ └─────┴─────┘
~~~

<h4>Proof</h4>

This rule is a direct consequence of the [no duplicate numbers rule](#ColumnNoDuplicateNumbers).

<h3 id="ColumnRemoveForeignRanges">Remove Foreign Ranges</h3>

<h4>Applicable if</h4>

In a column, there are two distinct blocks $B_i$ and $B_j$. A field $F$ in $B_i$ has a remaining possible value $v$ that lies in the [certain range](#certain-range) $\mathcal{R}_j$ of $B_j$.

<h4>Simplification</h4>

The number $v$ can be removed from $F$.

<h4>Example</h4>

~~~txt
┌───────┬─────┐ ┌─────┐ ┌─────────────┬───────────┐
│ 3 4 5 │ 3 4 │ │ 6 4 │ │ 1 3 4 5 6 7 │ 1 3 4 6 7 │
└───────┴─────┘ └─────┘ └─────────────┴───────────┘
~~~

where the first block has certain range

~~~math
\mathcal{R_1} = \{ 4 \}
~~~

becomes

~~~txt
┌───────┬─────┐ ┌───┐ ┌───────────┬─────────┐
│ 3 4 5 │ 3 4 │ │ 6 │ │ 1 3 5 6 7 │ 1 3 6 7 │
└───────┴─────┘ └───┘ └───────────┴─────────┘
~~~

<h4>Proof</h4>

Because of the [no duplicate numbers rule](#ColumnNoDuplicateNumbers), $v$ cannot occur in two solved fields. It must occur in the solution of $B_j$, $F$ is not in $B_j$, therefore $v$ cannot be part of the solution of $F$.

<h3 id="ColumnNFieldsWithNNumbers">N Fields With N Numbers</h3>

<h4>Applicable if</h4>

Within a column, there are $N$ (or fewer) fields whose remaining possible values form a set of $N$ numbers.

<h4>Simplification</h4>

These $N$ numbers can be removed from all other fields in the column.

<h4>Example</h4>

~~~txt
┌───────┬─────┐ ┌─────┐ ┌───────────────┬───────────┐
│ 3 4 6 │ 3 4 │ │ 6 3 │ │ 1 2 3 4 5 6 7 │ 1 3 4 6 7 │
└───────┴─────┘ └─────┘ └───────────────┴───────────┘
~~~

where the first three fields have a set of three values

~~~math
S = \{ 3, 4, 6 \}
~~~

becomes

~~~txt
┌───────┬─────┐ ┌─────┐ ┌─────────┬─────┐
│ 3 4 6 │ 3 4 │ │ 6 3 │ │ 1 2 5 7 │ 1 7 │
└───────┴─────┘ └─────┘ └─────────┴─────┘
~~~

<h4>Proof</h4>

Let $\mathcal{F}$ be the set of the $N$ fields with the $N$ numbers, and $\mathcal{N}$ be the set of the $N$ numbers. Both sets have $N$ elements.

Assume that another field $G \notin \mathcal{F}$ has a value $v \in \mathcal{N}$ when solved.

Then because of the [no duplicates rule](#ColumnNoDuplicateNumbers), $v$ cannot be a solution for one of the fields in $\mathcal{F}$. So we only have $N - 1$ possible solutions for $N$ fields. This is a contradiction, and so our assumption must be wrong.

<h3 id="ColumnConsistentRanges">Consistent Ranges</h3>

<h4>Definitions</h4>

For a block $B_i$ with $b_i$ fields,

* we define the set of remaining possible values $V_i$ as the union of the remaining possible values of all fields in the block, and
* we define the set of possible solution ranges $R_i$ as the set of all integer intervals of length $b_i$ that are contained in $V_i$, and
* we define the set of consistent solution ranges $C_i$ as the subset of those elements of $R_i$ that are part of at least one mutually non-intersecting combination of solution ranges of all blocks in the column.
* we define the consistency-refined value set $W_i$ as the union of all consistent solution ranges $C_i$ of block $B_i$.

<h4>Applicable if</h4>

Some of the possible values of the block $B_i$ are not part of a consistent column solution.

~~~math
W_i \neq V_i
~~~

<h4>Simplification</h4>

We can remove these values (all elements of $V_i \setminus W_i$) from block $B_i$.

<h4>Example</h4>

~~~txt
┌─────────┬─────┬───────────┐ ┌─────────┬─────┐
│ 2 3 4 5 │ 4 6 │ 2 3 4 5 7 │ │ 5 6 7 8 │ 6 7 │
└─────────┴─────┴───────────┘ └─────────┴─────┘
~~~

The blocks have the ranges:

~~~math
R_1 = \{ [2, 4], [3, 5], [4, 6], [5, 7] \},\ R_2 = \{ [5, 6], [6, 7], [7, 8] \}
~~~

We see that all the intervals in $R_2$ intersect with the $[5, 7]$ interval. Therefore, the consistent solution ranges of block 1 are given by

~~~math
C_1 = R_1 \setminus \{ [5, 7] \} = \{ [2, 4], [3, 5], [4, 6] \}
~~~

and the consistency-refined value set of block 1 is

~~~math
W_1 = \bigcup C_1 = [2, 6]
~~~

And because $ V_1 \setminus W_1 = \{ 7 \} $, we can remove the number 7 from the first block to get

~~~txt
┌─────────┬─────┬─────────┐ ┌─────────┬─────┐
│ 2 3 4 5 │ 4 6 │ 2 3 4 5 │ │ 5 6 7 8 │ 6 7 │
└─────────┴─────┴─────────┘ └─────────┴─────┘
~~~

<h4>Proof</h4>

The [consecutive numbers rule](#BlockConsecutiveNumbers) implies that the values of a solved block must be one of the solution ranges $R_i$. The [no duplicates rule](#ColumnNoDuplicateNumbers) says that the solution ranges of all blocks of a column do not intersect. Therefore, a value that does not occur in such a non-intersecting combination of solution ranges cannot be part of the solution.
