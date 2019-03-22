
ArrayList<Point> points = new ArrayList<Point>();
ArrayList<Point> pcmcs = new ArrayList<Point>();
ArrayList<Point> ocmcs = new ArrayList<Point>();
ArrayList<Point> pcmcPoints = new ArrayList<Point>();
ArrayList<Point> ocmcPoints = new ArrayList<Point>();

void setup() {
  size(600, 600);
  background(255);
  
  // points.add(new Point(250, 300, 5)); 
  // points.add(new Point(300, 250, 5));
  // points.add(new Point(300, 350, 5));
  // points.add(new Point(350, 300, 5));

  readPoints("data.synthetic", points);
  readClusters("ocmc", ocmcs);
  readClusters("ocmcPoints", ocmcPoints);
  readClusters("pcmc", pcmcs);
  readClusters("pcmcPoints", pcmcPoints);
  noStroke();
  fill(150, 150, 150);
  for (int i = 0; i < points.size(); i++) {
    points.get(i).show();
  }
  
  stroke(0);
  fill(255, 0, 0);
  for (int i = 0; i < ocmcPoints.size(); i++) {
    ocmcPoints.get(i).show();
  }
  
  fill(0, 200, 0);
  for (int i = 0; i < pcmcPoints.size(); i++) {
    pcmcPoints.get(i).show();
  }
  
  noFill();
  stroke(255, 0, 0);
  for (int i = 0; i < ocmcs.size(); i++) {
    ocmcs.get(i).show();
  }
  
  stroke(0, 200, 0);
    for (int i = 0; i < pcmcs.size(); i++) {
      pcmcs.get(i).show();
  }
}


int readPoints(String filename, ArrayList<Point> toAdd) {
  String[] lines = loadStrings(filename); 
  for (int i = 0; i < lines.length; i++) {
    String[] attrs = split(lines[i], ' ');
    float x = float(attrs[0]);
    float y = float(attrs[1]);
    toAdd.add(new Point(x, y, 2));
  }
  
  return lines.length;
}


int readClusters(String fileName, ArrayList<Point> toAdd) {
  String[] lines = loadStrings(fileName);
  for (int i = 0; i < lines.length; i++) {
    String[] attrs = split(lines[i], ' ');
    float x = float(attrs[0]);
    float y = float(attrs[1]);
    float r = float(attrs[2]);
    toAdd.add(new Point(x, y, r));
  }
  
  return lines.length;
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
