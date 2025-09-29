import 'react'

declare module 'react' {
  interface HTMLAttributes<T> extends AriaAttributes, DOMAttributes<T> {
    // allows for custom attributes
    [key: string]: any
  }
}