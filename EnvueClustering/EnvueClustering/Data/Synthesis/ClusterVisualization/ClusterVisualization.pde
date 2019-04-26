
ArrayList<Point> points = new ArrayList<Point>();
ArrayList<Point> pcmcs = new ArrayList<Point>();
ArrayList<Point> ocmcs = new ArrayList<Point>();
ArrayList<Point> pcmcPoints = new ArrayList<Point>();
ArrayList<Point> ocmcPoints = new ArrayList<Point>();

void setup() {
  size(600, 600);
  background(255);

  readRawPoints("data.synthetic.json", points);
  readMicroClusters("mcs.json", ocmcs);

  // Display raw points
  for (int i = 0; i < points.size(); i++) {
    points.get(i).show();
  }
  
  // Draw MCs as green circles
  noFill();
  stroke(0, 150, 0);
  for (int i = 0; i < ocmcs.size(); i++) {
    ocmcs.get(i).show();
  }
}

int readRawPoints (String filename, ArrayList<Point> toAdd) {
  JSONArray values = loadJSONArray(filename);
  for (int i = 0; i < values.size(); i++) {
    JSONObject jp = values.getJSONObject(i);
    toAdd.add(new Point(jp.getFloat("x"), jp.getFloat("y"), 2));
  }
  return values.size();
}

int readMicroClusters(String fromFile, ArrayList<Point> toAdd) {
  JSONArray values = loadJSONArray(fromFile);
  for (int i = 0; i < values.size(); i++) {
    // Get the center and the radius
    JSONObject jp = values.getJSONObject(i);
    float x = jp.getJSONObject("Center").getFloat("X");
    float y = jp.getJSONObject("Center").getFloat("Y");
    float r = jp.getFloat("Radius");

    toAdd.add(new Point(x, y, r));
  }
  return values.size();
}

int readPoints(String filename, ArrayList<Point> toAdd) {
  JSONArray values = loadJSONArray(filename);
  for (int i = 0; i < values.size(); i++) {
    JSONObject jp = values.getJSONObject(i);
    toAdd.add(new Point(jp.getFloat("X"), jp.getFloat("Y"), jp.getFloat("Radius")));
  }
  return values.size();
}

class Point {
  float x, y, r;
  public Point(float x, float y, float r) {
    this.x = x;
    this.y = y;
    this.r = r;
  }
  
  public void show() {
    ellipse(x, y, r * 2, r * 2);
  }
}
