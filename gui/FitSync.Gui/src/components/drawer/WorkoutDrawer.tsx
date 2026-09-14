import { Box, Drawer } from "@mui/material";
import { motion } from "motion/react";
import ActivityDrawerBody from "./ActivityDrawerBody";
import LinkedDrawerBody from "./LinkedDrawerBody";
import WorkoutDrawerBody from "./WorkoutDrawerBody";
import WorkoutDrawerFooter from "./WorkoutDrawerFooter";
import WorkoutDrawerHeader from "./WorkoutDrawerHeader";
import type { CalendarEventData } from "../../types/calendar";

const DRAWER_WIDTH = "min(600px, 100vw)";

interface WorkoutDrawerProps {
  event: CalendarEventData | null;
  onClose: () => void;
}

function DrawerBody({ event }: { event: CalendarEventData }) {
  if (event.kind === "activity") return <ActivityDrawerBody event={event} />;
  if (event.linkedActivityId) return <LinkedDrawerBody event={event} />;
  return (
    <WorkoutDrawerBody
      workoutId={event.workoutId}
      sport={event.sport}
      publications={event.publications}
    />
  );
}

export default function WorkoutDrawer({ event, onClose }: WorkoutDrawerProps) {
  return (
    <Drawer
      anchor="right"
      variant="temporary"
      open={event !== null}
      onClose={onClose}
      ModalProps={{ keepMounted: true }}
      sx={{
        "& .MuiDrawer-paper": {
          width: DRAWER_WIDTH,
          maxWidth: "100vw",
          boxSizing: "border-box",
        },
      }}
    >
      {event && (
        <motion.div
          key={event.id}
          initial={{ x: 48, opacity: 0 }}
          animate={{ x: 0, opacity: 1 }}
          transition={{ duration: 0.2, ease: "easeOut" }}
          style={{
            display: "flex",
            flexDirection: "column",
            height: "100%",
          }}
        >
          <WorkoutDrawerHeader event={event} onClose={onClose} />
          <Box sx={{ flexGrow: 1, overflowY: "auto", px: 3, py: 2 }}>
            <DrawerBody event={event} />
          </Box>
          <WorkoutDrawerFooter event={event} onClose={onClose} />
        </motion.div>
      )}
    </Drawer>
  );
}
