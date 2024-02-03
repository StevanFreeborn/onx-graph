export function toTitleCase(str: string) {
  // Insert a space between camel case words
  str = str.replace(/([a-z])([A-Z])/g, '$1 $2');

  // Capitalize the first letter of each word
  str = str.replace(/\w\S*/g, function (txt) {
    return txt.charAt(0).toUpperCase() + txt.substring(1).toLowerCase();
  });

  // Return the final title-cased string
  return str;
}
