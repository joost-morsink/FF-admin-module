# Fraction set

A fraction set is a technical data structure that keeps track of how big a share each part of a total has.
All the fractions in a fraction set should always add up to 1.
When entries are added into a fraction set, the set is automatically renormalized to add up to 1 again.
To eliminate error propagation due to repeated division, leading to non-sensical data, a `Divisor` is tracked in the data structure.

