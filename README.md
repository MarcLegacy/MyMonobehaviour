# MyMonobehaviour
Contains a base class and interface set that lets components hook into Awake/Start/Enable/Disable/Destroy and update callbacks without manually wiring Unity lifecycle methods.

MyMonoBehaviour is the core abstract base class. It registers implementing objects with UpdateManager when enabled and unregisters them when disabled, while also validating required serialized fields via reflection and custom attributes. The Interfaces folder defines the lifecycle contract.
