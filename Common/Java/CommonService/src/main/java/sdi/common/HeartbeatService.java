package src.main.java.sdi.common;

public class HeartbeatService implements Runnable {

    private final int interval = 5000; // Heartbeat interval in milliseconds

    @Override
    public void run() {
        while (true) {
            try {

                System.out.println("Heartbeat: Sent");
                Thread.sleep(interval); // Sleep for 5 seconds
            } catch (InterruptedException e) {
                System.err.println("Heartbeat service interrupted: " + e.getMessage());
                break;
            }
        }
    }
}
