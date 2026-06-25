Private constant LiteEndpoint stores the base URL for DuckDuckGo's Lite search endpoint and is used by the search logic to issue lightweight requests to the Lite service. Centralizing the URL as a const ensures the value remains immutable and consistently referenced within the class, reducing duplication and the risk of typos when constructing requests.

## Remarks

This symbol encapsulates transport endpoint concerns, isolating the actual URL from request-building logic. It helps maintain consistent usage of the Lite endpoint and makes it straightforward to update the URL in one place if needed. The explicit Lite endpoint also communicates the intended API surface used by the class.

## Notes

- Because it is a const, its value is baked into consuming assemblies at compile time. Changing the URL requires recompilation of dependents.
- It is private; if access from outside the class is ever required, consider widening the modifier or exposing it via a controlled accessor or configuration.
- When concatenating paths, be mindful of trailing slashes to avoid malformed URLs; prefer Uri or careful string handling.