<h1 id="BasicRulesOfStr8ts">Basic Rules of Str8ts</h1>

A Str8ts grid is a square grid of `n x n` **fields**. The fields are either black or white, and initially either blank or filled with a single number. A numbered white field is a known white field, a blank white field is an unknown white field. Initially, the unknown white fields can have any value in the range `1 .. n`. Only the unknown white fields can be edited, all other fields are fixed.

The goal of the game is to solve the unknown white fields; that is, to reduce the possible values of each white field to a single value. A grid always has a solution; usually a unique solution. We will not use the uniqueness of the solution for our simplification rules below.

An uninterrupted vertical or horizontal sequence of white fields forms a vertical or horizontal **Block**.

A complete column or row of fields in the grid forms a vertical or horizontal **Column**.

<h2>Block Rule</h2>

<h3 id="BlockConsecutiveNumbers">Consecutive Numbers</h3>

A solved block of `k` solved fields consists of exactly `k` consecutive numbers, i. e. all numbers `v` with `a ≤ v < a + k` in any order, for some number `a`.

<h2>Column Rule</h2>

<h3 id="ColumnNoDuplicateNumbers">No Duplicate Numbers</h3>

In grid of size `n`, a solved column (or row) may contain each of the numbers `1 .. n` at most once. It may not contain any other numbers.

<h1>Derived Simplfication Rules</h1>

The following derived [block rules](#derived-block-rules) and [column rules](#derived-column-rules) can be used to solve a str8ts grid, they are derived from the [basic game rules](#BasicRulesOfStr8ts).

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

In a block of size `k`,

* the mimimum of the possible values of field `F` is `ṃ`,
and another field `G` in the same block has a value `v` with `v ≤ ṃ - k`.

-or-

* the maximum of the possible values of field `F` is `ṁ`,
and another field `G` in the same block has a value `v` with `v ≥ ṁ + k`.

<h4>Simplification</h4>

`v` can be removed from `G`.

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

Let `M` be the maximum value in the solved block. Then `M ≥ ṃ`. There are `k - 1` other fields in the block.

Then, because of the [consecutive numbers rule](#BlockConsecutiveNumbers), the minimum of the values in the solved block is `M - (k - 1)`.

From `v ≤ ṃ - k` and `ṃ ≤ M`, we have `v ≤ M - k` and hence `v < M - (k - 1)`. Hence `v` cannot be in the solved block.

The proof for the second part is analogous.

<h3 id="BlockNoNeighbors">No Neighbors</h3>

This is a special case of the [disjunct subsets rule](#BlockDisjunctSubsets).

<h4>Applicable if</h4>

The block has more than one field. A field `F` has a number `a` and none of the other fields has one of the values `a + 1` or `a - 1`.

<h4>Simplification</h4>

The value `a` can be removed from `F`.

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

Within a block, there is a field `F` with exactly two remaining possible values, and the difference between these values is greater than or equal to the number `k` of fields in the block.

<h4>Simplification</h4>

The two values of `F` can be removed from all other fields in the block.

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

Let the two values of the field `F` be `a` and `b`, with `b ≥ a + k`.

Assume that another field `G` has the value `a` when solved. Then

* `F` must have the value `b` because of the [no duplicates rule](#ColumnNoDuplicateNumbers),
* `F`cannot have the value `b` because of the [minimum and maximum rule](#BlockMinimumAndMaximum).

So we have a contradiction and our assumption must be wrong.

The proof for `b` is analogous.

<h3 id="BlockDisjunctSubsets">Disjunct Subsets</h3>

<h4>Applicable if</h4>

* The remaining possible numbers of all fields in a block of size `k` form two or more disjunct subsets,
i. e. there are gaps in these numbers.

* One of these subsets `S1` has cardinality less than `k`, i. e. consists of fewer than `k` numbers.

<h4>Simplification</h4>

The numbers in `S1` kann be removed from all fields in the block.

<h4>Example</h4>

~~~txt
┌─────────┬─────────┬───────┐
│ 4 6 7 9 │ 3 4 7 8 │ 3 6 7 │
└─────────┴─────────┴───────┘
~~~

with disjunct subsets

~~~txt
S1 = { 3, 4 }, S2 = { 6, 7, 8, 9 }
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

<h4>Applicable if</h4>

* In a block, there is a set `C` of `N` distinct numbers that occur on exactly `N` fields.

* All numbers of `C` must occur in the solved block because they are in the <span id="certain-range">set intersection of all remaining possible solutions (= the *certain range* of the block)</span>.

<h4>Simplification</h4>

All other numbers can be removed from the fields with numbers from `C`.

<h4>Example</h4>

~~~txt
┌─────────┬───────────┬───────┬─────────┬─────┬─────┐
│ 3 6 7 9 │ 3 4 5 7 8 │ 6 7 9 │ 3 4 6 7 │ 7 8 │ 7 9 │
└─────────┴───────────┴───────┴─────────┴─────┴─────┘
~~~

with certain range

~~~txt
R = [ 3, 8 ] ∩ [ 4, 9 ] = [ 4, 8 ]
~~~

and where

~~~txt
C = { 4, 5 } ⊆ R
~~~

becomes

~~~txt
┌─────────┬─────┬───────┬───┬─────┬─────┐
│ 3 6 7 9 │ 4 5 │ 6 7 9 │ 4 │ 7 8 │ 7 9 │
└─────────┴─────┴───────┴───┴─────┴─────┘
~~~

<h4>Proof</h4>

We assume that one of the `N` fields with values from `C` has a value `v ∉ C` when solved. Then there remain `N - 1` possible fields where we can put the `N` elements of `C`, so one of the elements of `C` cannot be part of the solution. But we know that all the values in `C` are part of the solution. So, we have a contradiction and the assumption must be wrong.

<h2 id="derived-column-rules">Column Rules</h2>

<h3 id="ColumnRemoveSolvedNumbers">Remove Solved Numbers</h3>

<h4>Applicable if</h4>

The value `a` of a solved field `F` occurs in another (unsolved) field `G` in the same column.

<h4>Simplification</h4>

The value `a` can be removed from the unsolved field `G`.

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

In a column, there are two blocks `B` and `D`. A field `F` in `D` contains a number `v` from the [certain range](#certain-range) `C` of `B`.

<h4>Simplification</h4>

The number `v` can be removed from `F`.

<h4>Example</h4>

~~~txt
┌───────┬─────┐ ┌─────┐ ┌─────────────┬───────────┐
│ 3 4 5 │ 3 4 │ │ 6 4 │ │ 1 3 4 5 6 7 │ 1 3 4 6 7 │
└───────┴─────┘ └─────┘ └─────────────┴───────────┘
~~~

where the first block has certain range

~~~txt
C = { 4 }
~~~

becomes

~~~txt
┌───────┬─────┐ ┌───┐ ┌───────────┬─────────┐
│ 3 4 5 │ 3 4 │ │ 6 │ │ 1 3 5 6 7 │ 1 3 6 7 │
└───────┴─────┘ └───┘ └───────────┴─────────┘
~~~

<h4>Proof</h4>

Because of the [no duplicate numbers rule](#ColumnNoDuplicateNumbers),`v` cannot occur in two solved fields. It must occur in the solution of `B`, `F` is not in `B`, therefore `v` cannot be part of the solution of `F`.

<h3 id="ColumnNFieldsWithNNumbers">N Fields With N Numbers</h3>

<h3 id="ColumnConsistentRanges">Consistent Ranges</h3>
