// This provides the generate function
// from an API endpoint running on the server.
function load_generate () {
  async function generate (size, difficulty) {
    const response = await fetch(`/generate?gridSize=${generateGridSize}&difficulty=${difficulty - 1}`)
    const data = await response.json()
    return data
  }

  return generate
}
